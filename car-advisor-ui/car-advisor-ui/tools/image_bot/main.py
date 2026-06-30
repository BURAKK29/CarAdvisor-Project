# main.py
import time
import json
import csv
import os
import pyodbc
from normalizer import clean_text
from search import get_valid_image
from utils import load_cache, save_cache

CACHE_FILE = "image_cache.json"
LOG_FILE = "image_bot.log"
MISSING_CSV = "missing_images.csv"

# DB connection - edit connection string as needed
def get_connection():
    return pyodbc.connect(
        "DRIVER={ODBC Driver 17 for SQL Server};"
        "SERVER=DESKTOP-I5L5LLB;"
        "DATABASE=CarAdvisor;"
        "Trusted_Connection=yes;"
    )

# load cache
cache = load_cache(CACHE_FILE)

def log(msg):
    ts = time.strftime("%Y-%m-%d %H:%M:%S")
    line = f"[{ts}] {msg}"
    print(line)
    with open(LOG_FILE, "a", encoding="utf-8") as f:
        f.write(line + "\n")

def write_missing(row):
    header = ["Id","Brand","Model","StartYear","EndYear","BodyType"]
    file_exists = os.path.exists(MISSING_CSV)
    with open(MISSING_CSV, "a", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        if not file_exists:
            w.writerow(header)
        w.writerow(row)

def main():
    conn = get_connection()
    cursor = conn.cursor()

    rows = cursor.execute("""
        SELECT Id, Brand, Model, StartYear, EndYear, BodyType
        FROM CarGenerations
        WHERE ImageUrl IS NULL
    """).fetchall()

    log(f"{len(rows)} kayıt işlenecek")

    for r in rows:
        car_id, brand_raw, model_raw, sy, ey, body_raw = r
        brand = clean_text(brand_raw).title()
        model = clean_text(model_raw).title()
        body = clean_text(body_raw).title() if body_raw else ""

        # build prioritized queries: year+body, year, body, model, model+exterior
        queries = []
        if sy:
            queries.append(f"{brand} {model} {sy} {body}".strip())
            queries.append(f"{brand} {model} {sy}".strip())
        if body:
            queries.append(f"{brand} {model} {body}".strip())
        queries.append(f"{brand} {model}".strip())
        queries.append(f"{brand} {model} exterior".strip())

        log(f"\n🚗 {brand} {model} ({sy}) - body='{body}'")
        image_url = None

        # try cache first (query-based)
        for q in queries:
            if q in cache:
                cached = cache[q]
                log(f"   🔁 cache hit for '{q}' -> {cached}")
                # still verify cached URL? assume cache correct
                image_url = cached
                break

        if not image_url:
            # try queries
            for q in queries:
                log(f"   🔍 {q}")
                try:
                    image_url = get_valid_image(q, brand, model, sy, ey)
                except Exception as ex:
                    log(f"   ⚠ Exception while searching '{q}': {ex}")
                    image_url = None

                if image_url:
                    # store in cache for query
                    cache[q] = image_url
                    save_cache(CACHE_FILE, cache)
                    break

                time.sleep(0.6)  # polite rate

        if image_url:
            try:
                cursor.execute(
                    "UPDATE CarGenerations SET ImageUrl=? WHERE Id=?",
                    image_url, car_id
                )
                conn.commit()
                log(f"   ✅ KAYDEDİLDİ -> {image_url}")
            except Exception as ex:
                log(f"   ❌ DB update failed for Id={car_id}: {ex}")
        else:
            log("   ❌ Görsel yok, kaydedildi missing csv")
            write_missing([car_id, brand_raw, model_raw, sy, ey, body_raw])

    conn.close()
    log("Bitti.")

if __name__ == "__main__":
    main()
