# normalizer.py
import re

def clean_text(text):
    if not text:
        return ""
    # remove extra chars like [redesign], parentheses, slashes
    s = re.sub(r"\[.*?\]", "", text)
    s = re.sub(r"[()/,_]+", " ", s)
    s = re.sub(r"\s+", " ", s)
    return s.strip()
