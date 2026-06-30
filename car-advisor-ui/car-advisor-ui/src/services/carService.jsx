import axios from "axios";

// Backend adresin (Port numarasını kendi projene göre kontrol et, örn: 7158 veya 5000)
const API_URL = "http://localhost:5295/api/cars/"; 

export default class CarService {
    
    // Mevcut metodun (GetBrands)
    getBrands() {
        return axios.get(API_URL + "brands");
    }

    // 👇 YENİ METOT: Markaya göre jenerasyonları getirir
    getGenerationsByBrand(brand) {
        return axios.get(API_URL + "models/" + brand);
    }
}