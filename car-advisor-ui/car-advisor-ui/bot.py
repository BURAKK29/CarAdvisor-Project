import pyodbc
import requests
import time
import re

WIKI_API = "https://commons.wikimedia.org/w/api.php"

HEADERS = {"User-Agent": "CarAdvisorBot/2.0"}

def normalize_generation(gen):
    if not gen:
        return ""
    gen = gen.lower()
    gen = re.sub(r"\[.*?\]", "", gen)
    gen = gen.replace("generation", "")
    gen = gen.replace("facelift", "")
    gen = gen.replace("redesign", "")
    gen = gen.replace("(", "").replace(")", "")
    if "/" in gen:
        gen = gen.split("/")[0]
    return gen.strip().upper()

def wiki_search(query):
    params = {
        "action": "query",
        "generator": "search",
        "gsrsearch": query,
        "gsrlimit": 5,
        "prop": "imageinfo",
        "iiprop": "url",
        "format": "json"
    }

    r = requests.get(WIKI_API, params=params, headers=HEADERS)
    data = r.json()

    pages = data.get("query", {}).get("pages", {})
    for p in pages.values():
        info = p.get("imageinfo")
        if info:
            return info[0]["url"]
    return None

def try_find_image(brand, model, gen, body):
    searches = []

    if gen and body:
        searches.append(f"{brand} {model} {gen} {body}")
    if gen:
        searches.append(f"{brand} {model} {gen}")
    if body:
        searches.append(f"{brand} {model} {body}")
    searches.append(f"{brand}ավորմ {model}")

    for q in searches:
        print(f"   🔍 {q}")
        img = wiki_search(q)
        if img:
            return img

    return None

# ---------------- DB ----------------
conn = pyodbc.connect(
    "DRIVER={SQL Server};SERVER=DESKTOP-I5L5LLB;DATABASE=CarAdvisor;Trusted_Connection=yes;"
)
cursor = conn.cursor()

cursor.execute("""
SELECT Id, Brand, Model, GenerationName, BodyType
FROM CarGenerations
WHERE ImageUrl IS NULL
""")

rows = cursor.fetchall()
print(f"{len(rows)} kayıt işlenecek")

for r in rows:
    car_id, brand, model, gen_raw, body = r
    gen = normalize_generation(gen_raw)

    print(f"\n🚗 {brand} {model} {gen} {body}")

    image = try_find_image(brand, model, gen, body)

    if image:
        cursor.execute(
            "UPDATE CarGenerations SET ImageUrl=? WHERE Id=?",
            image, car_id
        )
        conn.commit()
        print("   ✅ KAYDEDİLDİ")
    else:
        print("   ❌ Bulunamadı")

    time.sleep(0.8)

conn.close()
