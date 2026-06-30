import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import './ModelDetailsPage.css';

export default function ModelDetailsPage() {
  const location = useLocation();
  const navigate = useNavigate();

  // Önceki sayfadan gönderdiğimiz araba kargosunu (state) teslim alıyoruz
  const { car, mainImage } = location.state || {};

  // Eğer sayfa yenilenirse (veri kaybolursa) geri gönderelim
  if (!car) {
    return (
      <div className="fallback-container">
        <h2>Araç verisi bulunamadı. Lütfen listeden tekrar seçim yapın.</h2>
        <button className="back-btn" onClick={() => navigate(-1)}>Geri Dön</button>
      </div>
    );
  }

  return (
    <div className="trim-page-container">
      {/* Üst Kısım: Başlık ve Geri Butonu */}
      <div className="trim-header-row">
        <button className="back-btn" onClick={() => navigate(-1)}>← Geri Dön</button>
        <h1 className="trim-main-title mt-4 mb-5">{car.make} {car.model} <span className='mx-2'>{car.trim}</span></h1>
      </div>

      {/* Hero Alanı: Resim ve Özet Bilgiler */}
      <div className="trim-hero-section">
        <div className="trim-image-container">
          <img src={mainImage} alt={`${car.make} ${car.model}`} className="trim-hero-img" referrerPolicy="no-referrer" />
        </div>

        <div className="trim-quick-info">
          <div className="big-price-tag">
            {car.averagePrice ? `${car.averagePrice.toLocaleString('tr-TR')} ₺` : 'Fiyat Bekleniyor'}
            <span className="price-label">Ortalama Piyasa Değeri</span>
          </div>

          <div className="quick-spec-badges">
            <span className="badge">{car.year} Model</span>
            <span className="badge">{car.bodyType}</span>
            <span className="badge">{car.transmission}</span>
            <span className="badge">{car.fuelType}</span>
          </div>

          <button className="big-compare-btn">⭐ Kıyaslama Listesine Ekle</button>
        </div>
      </div>

      {/* Detaylı Özellikler Izgarası (Grid) */}
      <div className="trim-details-grid">

        {/* Motor Kartı */}
        <div className="detail-card">
          <h3>⚙️ Motor ve Performans</h3>
          <ul>
            <li><span>Motor Hacmi:</span> <strong>{car.engineCC} cc</strong></li>
            <li><span>Beygir Gücü:</span> <strong>{car.horsePower} HP</strong></li>
            <li><span>0-100 Hızlanma:</span> <strong>{car.acceleration > 0 ? `${car.acceleration} sn` : 'Bilinmiyor'}</strong></li>
            <li><span>Maksimum Hız:</span> <strong>{car.maxSpeed > 0 ? `${car.maxSpeed} km/s` : 'Bilinmiyor'}</strong></li>
          </ul>
        </div>

        {/* Yakıt Kartı */}
        <div className="detail-card">
          <h3>⛽ Yakıt ve Tüketim</h3>
          <ul>
            <li>
              <span>Yakıt Tipi:</span>
              <strong>{car.fuelType || 'Bilinmiyor'}</strong>
            </li>
            <li>
              <span>Şehir İçi Tüketim:</span>
              <strong>
                {Number(car.cityFuelConsumption) > 0
                  ? `${car.cityFuelConsumption} ${car.fuelType === 'Electric' ? 'kWh/100km' : 'L/100km'}`
                  : 'Bilinmiyor'}
              </strong>
            </li>
            <li>
              <span>Ortalama Tüketim:</span>
              <strong>
                {Number(car.averageFuelConsumption) > 0
                  ? `${car.averageFuelConsumption} ${car.fuelType === 'Electric' ? 'kWh/100km' : 'L/100km'}`
                  : 'Bilinmiyor'}
              </strong>
            </li>
          </ul>
        </div>

        {/* Kasa Kartı */}
        <div className="detail-card">
          <h3>📐 Kasa ve Donanım</h3>
          <ul>
            <li><span>Kasa Tipi:</span> <strong>{car.bodyType}</strong></li>
            <li><span>Koltuk Sayısı:</span> <strong>{car.seats} Kişilik</strong></li>
            <li><span>Vites Tipi:</span> <strong>{car.transmission}</strong></li>
            <li><span>Üretim Yılı:</span> <strong>{car.year}</strong></li>
          </ul>
        </div>

      </div>
    </div>
  );
}