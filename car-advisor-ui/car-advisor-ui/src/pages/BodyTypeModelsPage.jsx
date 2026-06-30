import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import './BrandModelsPage.css'; // Eski CSS'i birebir kullanıyoruz, baştan yazmaya gerek yok!

export default function BodyTypeModelsPage() {
  const { bodyTypeName } = useParams();
  const [brandGroups, setBrandGroups] = useState({});
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const DEFAULT_CAR_IMAGE = "https://www.bmw-m.com/content/dam/bmw/marketBMW_M/www_bmw-m_com/topics/magazine-article-pool/2021/e46-gtr-street/bmw-m3-gtr-street-stage-teaser.jpg";

  useEffect(() => {
    // API'ye parametreli istek atıyoruz
    axios.get(`http://localhost:5295/api/Cars/getbybodytype`, { params: { bodyType: bodyTypeName } })
      .then(result => {
        // Yeniden eskiye sıralama
        const sortedData = result.data.sort((a, b) => b.startYear - a.startYear);
        // BÜYÜK FARK: Bu kez model ismine göre değil, MARKAYA göre grupluyoruz (Örn: BMW)
        const groups = groupByBrand(sortedData);
        setBrandGroups(groups);
        setLoading(false);
      })
      .catch(err => { console.log(err); setLoading(false); });
  }, [bodyTypeName]);

  const groupByBrand = (data) => {
    return data.reduce((acc, car) => {
      if (!acc[car.brand]) acc[car.brand] = [];
      acc[car.brand].push(car);
      return acc;
    }, {});
  };

  const getImageUrl = (url) => {
    if (!url || url === "NotFound" || url === "undefined" || url.trim() === "") return DEFAULT_CAR_IMAGE;
    if (url.startsWith("http")) return url;
    return `/car_images/${url}`;
  };

  const handleImageError = (e) => {
    if (e.target.src !== DEFAULT_CAR_IMAGE) {
      e.target.src = DEFAULT_CAR_IMAGE;
      e.target.style.padding = "20px"; 
      e.target.style.objectFit = "contain";
    }
  };

  if (loading) return (
    <div className="loading-container-premium"><div className="loader-premium"></div><p className="loading-text-premium">Yükleniyor...</p></div>
  );

  return (
    <div className="brand-models-wrapper-premium">
      <div className="brand-header-premium text-center">
        <h1 className="brand-page-title-premium">{bodyTypeName} Araçlar</h1>
        <p className="brand-header-subtitle-premium">Tüm markaların {bodyTypeName} kasa tipine ait modellerini keşfedin</p>
      </div>

      <div className="container custom-container-premium">
        {/* Markaları Alfabetik Sıralıyoruz (Audi, BMW, Citroen...) */}
        {Object.keys(brandGroups).sort((a, b) => a.localeCompare(b)).map(brandName => (
          <div key={brandName} className="premium-model-section-updated">
            
            <div className="section-header-updated">
              <h2 className="premium-model-title-updated">{brandName}</h2> {/* Marka Başlığı */}
              <div className="title-underline-updated"></div>
            </div>
            
            <div className="premium-carousel-container-updated">
              <div className="premium-carousel-track-updated">
                  {brandGroups[brandName].map(car => (
                  <div key={car.id} 
                    className="premium-car-card-updated"
                    onClick={() => navigate(`/details/${car.brand}/${car.model}/${encodeURIComponent(car.generationName)}/${encodeURIComponent(car.bodyType)}`)}                
                  >
                      <div className="premium-image-wrapper-updated">
                        <img 
                            src={getImageUrl(car.imageUrl)} 
                            alt={`${car.brand} ${car.model}`} 
                            className="premium-car-image-updated"
                            onError={handleImageError} 
                            referrerPolicy="no-referrer"
                        />
                        <span className="premium-year-badge-updated">{car.startYear} - {car.endYear || "Devam"}</span>
                      </div>
                      
                      <div className="premium-card-info-updated">
                        <h3 className="premium-generation-name-updated" title={car.generationName}>
                            {car.model} {/* Kartın üstünde model ismi (Örn: 5 Series) yazacak */}
                        </h3>
                        <div className="premium-tags-updated">
                            <span className="premium-tag-updated">{car.generationName}</span>
                        </div>
                      </div>
                  </div>
                  ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}