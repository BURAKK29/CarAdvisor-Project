import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import "./TrimPackagesPage.css";
import { useCompare } from "../context/CompareContext";
export default function TrimPackagesPage() {
  const { brandName, modelName, generationName, bodyType } = useParams();
  const { addToCompare, compareList } = useCompare();
  const [carDetails, setCarDetails] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const decodedGeneration = decodeURIComponent(generationName);
  const decodedBodyType = decodeURIComponent(bodyType);
  const isAlreadyAdded = compareList.some(
    (c) =>
      c.make === brandName &&
      c.model === modelName &&
      c.generation === generationName &&
      c.bodyType === bodyType,
  );
  useEffect(() => {
    axios
      .get(`http://localhost:5295/api/Cars/getcardetails`, {
        params: {
          brand: brandName,
          model: modelName,
          generation: decodedGeneration,
          bodyType: decodedBodyType,
        },
      })
      .then((response) => {
        setCarDetails(response.data);
        setLoading(false);
      })
      .catch((error) => {
        console.error("Detaylar çekilemedi:", error);
        setLoading(false);
      });
  }, [brandName, modelName, decodedGeneration, decodedBodyType]);

  if (loading)
    return (
      <div className="loading-container-premium">
        <div className="loader-premium"></div>
        <p className="loading-text-premium">Donanım paketleri yükleniyor...</p>
      </div>
    );

  if (!carDetails || carDetails.length === 0)
    return (
      <div className="premium-error-container">
        <div className="error-icon">⚠️</div>
        <h2 className="error-title">Donanım Bulunamadı</h2>
        <p className="error-text">
          Bu araca ait donanım detayı henüz sisteme eklenmemiş.
        </p>
        <button className="premium-back-btn mt-4" onClick={() => navigate(-1)}>
          ← Geri Dön
        </button>
      </div>
    );

  const mainImage =
    carDetails[0].imageUrl && carDetails[0].imageUrl.startsWith("http")
      ? carDetails[0].imageUrl
      : "https://www.bmw-m.com/content/dam/bmw/marketBMW_M/www_bmw-m_com/topics/magazine-article-pool/2021/e46-gtr-street/bmw-m3-gtr-street-stage-teaser.jpg";

  return (
    <div className="premium-details-wrapper">
      {/* ÜST BİLGİ ALANI */}
      <div className="premium-details-header">
        <div className="container custom-container-premium">
          <button className="premium-back-btn" onClick={() => navigate(-1)}>
            <span className="back-arrow">←</span> Modeller'e Dön
          </button>
          <div className="header-titles">
            <h1 className="premium-car-title">
              {brandName} {modelName}
            </h1>
            <h2 className="premium-generation-subtitle">
              {decodedGeneration} Serisi Donanım Paketleri
            </h2>
          </div>
        </div>
      </div>

      {/* DONANIM (TRIM) KARTLARI */}
      <div className="container custom-container-premium">
        <div className="premium-listing-container">
          {carDetails.map((trim, index) => (
            <div key={index} className="premium-trim-card">
              <div className="premium-trim-image-box">
                <img
                  src={mainImage}
                  alt={`${trim.make} ${trim.model}`}
                  className="premium-trim-img"
                />
              </div>

              <div className="premium-trim-info-box">
                <h3 className="premium-trim-title">
                  {trim.make} {trim.model}{" "}
                  <span className="trim-highlight">{trim.trim}</span>
                </h3>

                {/* ÖZELLİKLER HAP (PILL) TASARIMINA GEÇTİ */}
                <div className="premium-trim-specs">
                  <div className="spec-pill">
                    <span className="spec-label">Yıl</span>{" "}
                    <span className="spec-value">{trim.year}</span>
                  </div>
                  <div className="spec-pill">
                    <span className="spec-label">Motor</span>{" "}
                    <span className="spec-value">{trim.engineCC} cc</span>
                  </div>
                  <div className="spec-pill">
                    <span className="spec-label">Güç</span>{" "}
                    <span className="spec-value">{trim.horsePower} HP</span>
                  </div>
                  <div className="spec-pill">
                    <span className="spec-label">Vites</span>{" "}
                    <span className="spec-value">{trim.transmission}</span>
                  </div>
                  <div className="spec-pill">
                    <span className="spec-label">Yakıt</span>{" "}
                    <span className="spec-value">{trim.fuelType}</span>
                  </div>
                  <div className="spec-pill">
                    <span className="spec-label">Kasa</span>{" "}
                    <span className="spec-value">{trim.bodyType}</span>
                  </div>
                </div>
              </div>

              <div className="premium-trim-action-box">
                <div className="price-wrapper">
                  <span className="price-label">Ortalama Piyasa Değeri</span>
                  {trim.averagePrice ? (
                    <div className="premium-price-tag">
                      {trim.averagePrice.toLocaleString("tr-TR")} ₺
                    </div>
                  ) : (
                    <div className="premium-price-tag pending">
                      Analiz Ediliyor
                    </div>
                  )}
                </div>

                <div className="action-buttons">
                  <button
                    className="premium-btn-primary"
                    onClick={() =>
                      navigate("/trim-details", {
                        state: { car: trim, mainImage: mainImage },
                      })
                    }
                  >
                    Araç Detayları
                  </button>
                  <button
                    className="premium-btn-secondary"
                    onClick={() => {
                      // Butona basıldığında Context'e aracın bilgilerini yolluyoruz
                      addToCompare({
                        make: brandName, 
                        model: modelName, 
                        generation: generationName, 
                        bodyType: trim.bodyType,
                        trim: trim.trim
                      });
                    }}
                  >
                    Kıyasla
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
