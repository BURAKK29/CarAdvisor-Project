import { useEffect, useState } from 'react';
import axios from 'axios';
import { Link } from 'react-router-dom';
import './HomePage.css';

const brandLogoMap = {
  Audi: "audi.svg", BMW: "bmw.svg", Citroen: "citroen.svg", Dacia: "dacia.svg",
  Fiat: "fiat.svg", Ford: "ford.svg", Honda: "honda.svg", Hyundai: "hyundai.svg",
  Kia: "kia.svg", Mazda: "mazda.svg", "Mercedes-Benz": "mercedes-benz.svg",
  Nissan: "nissan.svg", Opel: "opel.svg", Peugeot: "peugeot.svg", Renault: "renault.svg",
  Skoda: "skoda.svg", Suzuki: "suzuki.svg", Toyota: "toyota.svg", Volkswagen: "volkswagen.svg",
  Volvo: "volvo.svg", Tesla: "tesla.svg"
};

// Kasa Tipleri İçin Statik Veri (İkon/Emoji ile)
const bodyTypes = [
  { name: "Sedan", icon: "🚗", desc: "Konforlu Aile Araçları" },
  { name: "SUV", icon: "🚙", desc: "Yüksek ve Güvenli" },
  { name: "Hatchback", icon: "🚘", desc: "Şehir İçi Pratiklik" },
  { name: "Station Wagon", icon: "🚐", desc: "Geniş Bagaj Hacmi" },
  { name: "Coupe", icon: "🏎️", desc: "Sportif Çizgiler" },
  { name: "Minivan", icon: "🚐", desc: "Geniş Aileler İçin" }
];

export default function HomePage() {
  const [brands, setBrands] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState(""); 
  const [activeTab, setActiveTab] = useState("brand"); // TAB YÖNETİMİ İÇİN STATE

  useEffect(() => {
    axios.get("http://localhost:5295/api/Cars/getbrands")
      .then((res) => { setBrands(res.data); setLoading(false); })
      .catch((err) => { console.error("Hata:", err); setLoading(false); });
  }, []);

  const filteredBrands = brands.filter(b => b.toLowerCase().includes(searchTerm.toLowerCase()));

  if (loading) return <div className="loading-container-premium"><div className="loader-premium"></div></div>;

  return (
    <div className="brand-page-wrapper">
      <div className="hero-section">
        <div className="hero-content text-center">
          <h1 className="hero-title text-light">Hayalindeki Aracı Keşfet</h1>
          <p className="hero-subtitle">Binlerce donanım, teknik detay ve güncel piyasa analizleri.</p>
          
          {/* TAB (SEKME) BUTONLARI */}
          <div className="premium-tabs-container">
            <button 
              className={`premium-tab-btn ${activeTab === 'brand' ? 'active' : ''}`}
              onClick={() => setActiveTab('brand')}
            >
              🏢 Markaya Göre Keşfet
            </button>
            <button 
              className={`premium-tab-btn ${activeTab === 'bodyType' ? 'active' : ''}`}
              onClick={() => setActiveTab('bodyType')}
            >
              🚙 Kasa Tipine Göre Keşfet
            </button>
          </div>

          {/* ARAMA KUTUSU SADECE MARKA SEKMESİNDEYKEN GÖRÜNÜR */}
          {activeTab === 'brand' && (
            <div className="search-box-container mt-4">
              <span className="search-icon">🔍</span>
              <input 
                type="text" 
                className="form-control premium-search-input" 
                placeholder="Hangi markayı arıyorsunuz? (Örn: Audi...)"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          )}
        </div>
      </div>

      <div className="container premium-brand-container">
        <div className="row g-4">
          
          {/* MARKALARI LİSTELE */}
          {activeTab === 'brand' && (
            filteredBrands.length > 0 ? (
              filteredBrands.map((brand, index) => (
                <div key={index} className="col-6 col-md-4 col-lg-3">
                  <Link to={`/model/${brand}`} className="text-decoration-none"> 
                    <div className="card h-100 premium-brand-card">
                      <div className="logo-wrapper">
                        <img src={`/src/assets/logos/${brandLogoMap[brand]}`} className="brand-logo-img" alt={brand} />
                      </div>
                      <div className="card-body text-center brand-card-body">
                        <h5 className="brand-name">{brand}</h5>
                        <span className="explore-btn">Modelleri İncele <i className="arrow-icon">→</i></span>
                      </div>
                    </div>
                  </Link>
                </div>
              ))
            ) : (<div className="col-12 text-center mt-5"><h4 className="text-muted">Bulunamadı.</h4></div>)
          )}

          {/* KASA TİPLERİNİ LİSTELE */}
          {activeTab === 'bodyType' && (
            bodyTypes.map((type, index) => (
              <div key={index} className="col-12 col-md-6 col-lg-4">
                <Link to={`/body-type/${type.name}`} className="text-decoration-none">
                  <div className="card h-100 premium-brand-card body-type-card">
                    <div className="body-type-icon-wrapper">
                      <span className="body-type-emoji">{type.icon}</span>
                    </div>
                    <div className="card-body text-center brand-card-body">
                      <h5 className="brand-name">{type.name}</h5>
                      <p className="text-muted small mb-2">{type.desc}</p>
                      <span className="explore-btn">Araçları Gör <i className="arrow-icon">→</i></span>
                    </div>
                  </div>
                </Link>
              </div>
            ))
          )}

        </div>
      </div>
    </div>
  );
}