import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { useCompare } from '../context/CompareContext';
import ReactMarkdown from 'react-markdown';
import './ComparePage.css';

export default function ComparePage() {
    const { compareList, removeFromCompare } = useCompare();
    const [carData, setCarData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [aiData, setAiData] = useState(null);
    const [isAiLoading, setIsAiLoading] = useState(false);
    const navigate = useNavigate();

    const callAiExpert = async () => {
        try {
            setIsAiLoading(true);
            const response = await axios.post('http://localhost:5295/api/Ai/compare-expert', carData);
            setAiData(response.data);
        } catch (err) {
            console.error("AI Kıyaslama hatası:", err);
            alert("Yapay Zeka analizi sırasında hata oluştu. " + (err.response?.data?.error || ""));
        } finally {
            setIsAiLoading(false);
        }
    };

    useEffect(() => {
        const fetchCompareData = async () => {
            if (compareList.length === 0) {
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                // C# tarafında yazdığımız compare endpoint'ine listeyi gönderiyoruz
                const response = await axios.post('http://localhost:5295/api/Cars/compare', compareList);
                setCarData(response.data);
                setError(null);
            } catch (err) {
                console.error("Kıyaslama verisi çekilemedi:", err);
                setError("Araç verileri yüklenirken bir sorun oluştu.");
            } finally {
                setLoading(false);
            }
        };

        fetchCompareData();
    }, [compareList]);

    // Resim URL'sini düzenleyen yardımcı fonksiyon
    const getImageUrl = (url) => {
        if (!url || url === "NotFound" || url === "undefined") {
            return "https://www.bmw-m.com/content/dam/bmw/marketBMW_M/www_bmw-m_com/topics/magazine-article-pool/2021/e46-gtr-street/bmw-m3-gtr-street-stage-teaser.jpg";
        }
        if (url.startsWith("http")) return url;
        return `/car_images/${url}`;
    };

    if (compareList.length === 0) {
        return (
            <div className="compare-empty-state">
                <h2>Kıyaslanacak Araç Yok</h2>
                <p>Lütfen karşılaştırmak için en az 2 araç seçin.</p>
                <button onClick={() => navigate('/')} className="btn-back-home">Modelleri Keşfet</button>
            </div>
        );
    }

    if (loading) {
        return (
            <div className="compare-loading">
                <div className="spinner"></div>
                <p>Araç verileri hazırlanıyor, lütfen bekleyin...</p>
            </div>
        );
    }

    if (error) {
        return <div className="compare-error">{error}</div>;
    }

    // Özellik satırlarını render etmek için yardımcı fonksiyon
    const renderRow = (label, key, formatFn = (val, car) => val) => (
        <tr key={key || label}>
            <td className="feature-label">{label}</td>
            {carData.map((car, index) => (
                <td key={index} className="feature-value">
                    {formatFn(key ? car[key] : null, car)}
                </td>
            ))}
        </tr>
    );

    return (
        <div className="compare-page-wrapper">
            <div className="compare-header-section">
                <button onClick={() => navigate(-1)} className="back-btn">← Geri Dön</button>
                <h1 className="compare-title">Araç Karşılaştırma</h1>
                <p className="compare-subtitle">Seçtiğiniz {carData.length} aracın donanım ve teknik özellikleri</p>

                {!aiData && (
                    <button 
                        className={`btn-ai-expert ${isAiLoading ? 'loading' : ''}`}
                        onClick={callAiExpert}
                        disabled={isAiLoading}
                    >
                        {isAiLoading ? 'Yapay Zeka Test Ediyor...' : '✨ Baş Uzman\'a Kıyaslat (AI)'}
                    </button>
                )}
            </div>

            <div className="compare-table-container">
                <table className="compare-table">
                    <thead>
                        {/* 1. SATIR: RESİMLER VE TEMEL BİLGİLER */}
                        <tr>
                            <th className="empty-corner">Özellikler</th>
                            {carData.map((car, index) => (
                                <th key={index} className="car-card-header">
                                    <button 
                                        className="remove-car-btn" 
                                        onClick={() => removeFromCompare(car)}
                                        title="Kıyaslamadan Çıkar"
                                    >
                                        ✕
                                    </button>
                                    <div className="car-image-container">
                                        <img src={getImageUrl(car.imageUrl)} alt={`${car.make} ${car.model}`} />
                                    </div>
                                    <h3 className="car-name">{car.make} {car.model}</h3>
                                    <span className="car-generation">{car.generation}</span>
                                    <div className="car-price">
                                        {car.averagePrice > 0 ? `${car.averagePrice.toLocaleString('tr-TR')} ₺` : 'Fiyat Bekleniyor'}
                                    </div>
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        <tr className="section-title-row">
                            <td colSpan={carData.length + 1}>⚙️ Motor ve Performans</td>
                        </tr>
                        {renderRow('Motor Hacmi', 'engineCC', (val) => val ? `${val} cc` : '-')}
                        {renderRow('Beygir Gücü', 'horsePower', (val) => val ? `${val} HP` : '-')}
                        {renderRow('0-100 Hızlanma', 'acceleration', (val) => val ? `${val} sn` : '-')}
                        {renderRow('Maksimum Hız', 'maxSpeed', (val) => val ? `${val} km/s` : '-')}

                        <tr className="section-title-row">
                            <td colSpan={carData.length + 1}>⛽ Yakıt ve Tüketim</td>
                        </tr>
                        {renderRow('Yakıt Tipi', 'fuelType')}
                        {renderRow('Ortalama Tüketim', 'averageFuelConsumption', (val) => val ? `${val.toFixed(1)} L / 100km` : '-')}

                        <tr className="section-title-row">
                            <td colSpan={carData.length + 1}>📐 Kasa ve Donanım</td>
                        </tr>
                        {renderRow('Kasa Tipi', 'bodyType')}
                        {renderRow('Vites Tipi', 'transmission')}
                        {renderRow('Bagaj Hacmi', 'trunkCapacity', (val) => val ? `${val} Litre` : '-')}
                        {renderRow('Üretim Yılı', 'year')}

                        {aiData && aiData.carEvaluations && (
                            <>
                                <tr className="section-title-row ai-section-title">
                                    <td colSpan={carData.length + 1}>🛡️ Sürüş ve Güvenlik (Yapay Zeka)</td>
                                </tr>
                                {renderRow('Yol Tutuş', '', (val, car) => {
                                    const mk = `${car.make} ${car.model}`;
                                    return aiData.carEvaluations[mk]?.handling || '-';
                                })}
                                {renderRow('Süspansiyon Konforu', '', (val, car) => {
                                    const mk = `${car.make} ${car.model}`;
                                    return aiData.carEvaluations[mk]?.comfort || '-';
                                })}
                                {renderRow('Güvenlik', '', (val, car) => {
                                    const mk = `${car.make} ${car.model}`;
                                    return aiData.carEvaluations[mk]?.safety || '-';
                                })}
                            </>
                        )}
                    </tbody>
                </table>
            </div>

            {(() => {
                if (!aiData || !aiData.expertVerdict) return null;

                let winnerCarObj = null;
                if (aiData.winnerCarFullName) {
                    winnerCarObj = carData.find(c => {
                        const fullName = `${c.make} ${c.model}`.toLowerCase();
                        const winnerName = aiData.winnerCarFullName.toLowerCase();
                        return fullName.includes(winnerName) || winnerName.includes(fullName);
                    });
                }

                return (
                    <div className="ai-verdict-card">
                        <h2 className="ai-verdict-title">✨ Baş Uzman'ın Kıyaslama Raporu</h2>
                        <div className={winnerCarObj ? "ai-verdict-split" : "ai-verdict-content"}>
                            <div className="ai-verdict-text">
                                <ReactMarkdown>{aiData.expertVerdict}</ReactMarkdown>
                            </div>
                            {winnerCarObj && (
                                <div className="ai-winner-showcase">
                                    <span className="winner-badge">🏆 Baş Uzmanın Seçimi</span>
                                    <img src={getImageUrl(winnerCarObj.imageUrl)} alt="Kazanan Araç" className="winner-car-img" />
                                    <h3 className="winner-car-name">{winnerCarObj.make} {winnerCarObj.model}</h3>
                                    <p className="winner-car-trim">{winnerCarObj.trim || winnerCarObj.generation}</p>
                                </div>
                            )}
                        </div>
                    </div>
                );
            })()}
        </div>
    );
}