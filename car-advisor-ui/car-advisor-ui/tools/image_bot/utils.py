import re

def extract_years(text):
    return [int(y) for y in re.findall(r"(19\d{2}|20\d{2})", text)]

def is_image_valid(title, brand, model, start_year, end_year):
    t = title.lower()

    # 1️⃣ MODEL ZORUNLU
    model_token = model.lower()
    if model_token not in t:
        return False

    # Audi A1 / A3 / A4 karışmasını önler
    if re.search(rf"\b{model_token}\b", t) is None:
        return False

    # 2️⃣ YIL KONTROLÜ
    years = extract_years(title)
    if not years:
        return False

    if len(years) == 1:
        return start_year <= years[0] <= end_year

    return min(years) >= start_year and max(years) <= end_year
