import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import CarService from '../services/carService';
import './BrandModelsPage.css';
import { useNavigate } from 'react-router-dom';

export default function BrandModelsPage() {
  const { brandName } = useParams();
  const [modelGroups, setModelGroups] = useState({});
  const [loading, setLoading] = useState(true);

  // 1. YENİ EKLENEN STATE: Arama metnini tutacak değişken
  const [searchTerm, setSearchTerm] = useState("");

  const navigate = useNavigate();

  const DEFAULT_CAR_IMAGE = "https://www.bmw-m.com/content/dam/bmw/marketBMW_M/www_bmw-m_com/topics/magazine-article-pool/2021/e46-gtr-street/bmw-m3-gtr-street-stage-teaser.jpg";

  useEffect(() => {
    let carService = new CarService();

    carService.getGenerationsByBrand(brandName)
      .then(result => {
        const sortedData = result.data.sort((a, b) => b.startYear - a.startYear);
        const groups = groupByModel(sortedData);
        setModelGroups(groups);
        setLoading(false);
      })
      .catch(err => {
        console.log(err);
        setLoading(false);
      });
  }, [brandName]);

  const groupByModel = (data) => {
    return data.reduce((acc, car) => {
      if (!acc[car.model]) {
        acc[car.model] = [];
      }
      acc[car.model].push(car);
      return acc;
    }, {});
  };

  const getImageUrl = (url) => {
    if (!url || url === "NotFound" || url.trim() === "") {
      return DEFAULT_CAR_IMAGE;
    }
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
    <div className="loading-container-premium">
      <div className="loader-premium"></div>
      <p className="loading-text-premium">Yükleniyor... Lütfen Bekleyin</p>
    </div>
  );

  // 2. YENİ EKLENEN FİLTRELEME MANTIĞI:
  // Mevcut model gruplarını alıp, kullanıcının girdiği kelimeyi aramada süzüyoruz.
  const filteredModels = Object.keys(modelGroups)
    .sort((a, b) => a.localeCompare(b))
    .filter(modelName => modelName.toLowerCase().includes(searchTerm.toLowerCase()));

  return (
    <div className="brand-models-wrapper-premium">
      <div className="brand-header-premium text-center">
        <h1 className="brand-page-title-premium">{brandName} Modelleri</h1>
        <p className="brand-header-subtitle-premium">Tüm nesiller, donanımlar ve teknik özellikler</p>

        {/* 3. YENİ EKLENEN ARAMA ÇUBUĞU (Ana sayfa ile aynı stil) */}
        <div className="search-box-container mt-4" style={{ maxWidth: '500px', margin: '0 auto' }}>
          <span className="search-icon">🔍</span>
          <input
            type="text"
            className="form-control premium-search-input"
            placeholder={`Hangi ${brandName} modelini arıyorsunuz?`}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="container custom-container-premium px-5 mt-4">
        {filteredModels.length > 0 ? (
          filteredModels.map(modelName => (
            <div key={modelName} className="premium-model-section-updated">

              <div className="section-header-updated">
                <h2 className="premium-model-title-updated">{modelName} Serisi</h2>
                <div className="title-underline-updated"></div>
              </div>

              <div className="premium-carousel-container-updated">
                <div className="premium-carousel-track-updated">
                  {modelGroups[modelName].map(car => (
                    <div key={car.id}
                      className="premium-car-card-updated"
                      onClick={() => navigate(`/details/${brandName}/${car.model}/${encodeURIComponent(car.generationName)}/${encodeURIComponent(car.bodyType)}`)}
                    >
                      <div className="premium-image-wrapper-updated">
                        <img
                          src={getImageUrl(car.imageUrl)}
                          alt={`${car.brand} ${car.model}`}
                          className="premium-car-image-updated"
                          onError={handleImageError}
                          referrerPolicy="no-referrer"
                        />
                        <span className="premium-year-badge-updated">
                          {car.startYear} - {car.endYear || "Devam"}
                        </span>
                      </div>

                      <div className="premium-card-info-updated">
                        <h3 className="premium-generation-name-updated" title={car.generationName || `${car.model} (${car.startYear})`}>
                          {car.generationName || `${car.model} (${car.startYear})`}
                        </h3>
                        <div className="premium-tags-updated">
                          <span className="premium-tag-updated">{car.bodyType}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="text-center mt-5">
            <h4 className="text-muted">Bu markaya ait aradığınız model bulunamadı.</h4>
          </div>
        )}
      </div>
    </div>
  );
}
