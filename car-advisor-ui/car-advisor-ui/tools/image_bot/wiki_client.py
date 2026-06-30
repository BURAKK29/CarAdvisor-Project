# wiki_client.py
import requests
import time

WIKI_API = "https://commons.wikimedia.org/w/api.php"
HEADERS = {"User-Agent": "CarAdvisorBot/1.0 (contact@example.com)"}

def safe_get(params, timeout=10):
    """Request with basic retry and JSON safety."""
    max_retries = 3
    backoff = 1.0
    for attempt in range(max_retries):
        try:
            r = requests.get(WIKI_API, params=params, headers=HEADERS, timeout=timeout)
            # if response empty or not JSON, raise to retry
            if r.status_code != 200:
                time.sleep(backoff)
                backoff *= 2
                continue
            # check content-type
            ct = r.headers.get("Content-Type", "")
            if "application/json" not in ct and not r.text.strip().startswith("{"):
                # sometimes returns HTML on error - treat as retryable
                time.sleep(backoff)
                backoff *= 2
                continue
            data = r.json()
            return data
        except requests.exceptions.RequestException:
            time.sleep(backoff)
            backoff *= 2
        except ValueError:
            # JSON decode error
            time.sleep(backoff)
            backoff *= 2
    return None

def search_pages(query, limit=8):
    params = {
        "action": "query",
        "list": "search",
        "srsearch": query,
        "srlimit": limit,
        "format": "json"
    }
    data = safe_get(params)
    if not data:
        return []
    results = data.get("query", {}).get("search", [])
    titles = [item.get("title") for item in results]
    return titles

def get_images_for_page(title):
    """Return list of file titles on a page (File:...)."""
    params = {
        "action": "query",
        "titles": title,
        "prop": "images",
        "format": "json"
    }
    data = safe_get(params)
    if not data:
        return []
    pages = data.get("query", {}).get("pages", {})
    file_titles = []
    for p in pages.values():
        for img in p.get("images", []):
            file_titles.append(img["title"])
    return file_titles

def get_image_url(file_title):
    params = {
        "action": "query",
        "titles": file_title,
        "prop": "imageinfo",
        "iiprop": "url|extmetadata",
        "format": "json"
    }
    data = safe_get(params)
    if not data:
        return None
    pages = data.get("query", {}).get("pages", {})
    for p in pages.values():
        imageinfo = p.get("imageinfo")
        if imageinfo:
            # return URL and file title
            return {"title": file_title, "url": imageinfo[0].get("url")}
    return None
