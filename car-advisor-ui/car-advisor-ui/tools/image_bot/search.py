from wiki_client import get_page_images
from utils import is_image_valid

def get_valid_image(search_query, brand, model, start_year, end_year):
    images = get_page_images(search_query)

    if not images:
        return None

    for img in images:
        if is_image_valid(
            img["title"],
            brand,
            model,
            start_year,
            end_year
        ):
            return img["url"]

    return None
