import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useCompare } from '../context/CompareContext';
import './CompareWidget.css';

export default function CompareWidget() {
    const { compareList, clearCompare } = useCompare();
    const navigate = useNavigate();
    const location = useLocation(); // Hangi sayfada olduğumuzu yakalıyoruz
    const [isOpen, setIsOpen] = useState(false); // Widget kapalı olarak başlar

    // 1. HARİKA ÇÖZÜM: Karşılaştırma sayfasındaysan widget'ı tamamen gizle!
    if (location.pathname === '/compare') return null;

    // Listede araç yoksa zaten gösterme
    if (compareList.length === 0) return null;

    // 2. HARİKA ÇÖZÜM: Widget "Kapalı" durumdaysa sadece minik bir buton göster
    if (!isOpen) {
        return (
            <button className="compare-widget-collapsed" onClick={() => setIsOpen(true)}>
                ⚖️ Kıyasla
                <span className="compare-badge">{compareList.length}</span>
            </button>
        );
    }

    // Widget "Açık" durumu
    return (
        <div className="compare-floating-widget">
            <div className="compare-widget-header">
                <span className="compare-count">Kıyaslanacak: <strong>{compareList.length}/4</strong></span>
                <div className="compare-header-actions">
                    <button className="compare-clear-btn" onClick={clearCompare}>Temizle</button>
                    {/* Kapatma Çarpısı Eklendi */}
                    <button className="compare-close-btn" onClick={() => setIsOpen(false)}>✖</button>
                </div>
            </div>
            
            <div className="compare-widget-list">
                {compareList.map((car, index) => (
                    <div key={index} className="compare-widget-item">
                        🚗 {car.make} {car.model}
                    </div>
                ))}
            </div>

            <button 
                className={`compare-go-btn ${compareList.length > 1 ? 'ready' : ''}`}
                onClick={() => {
                    if (compareList.length > 1) {
                        setIsOpen(false); // Giderken widget'ı kapatalım
                        navigate('/compare');
                    } else {
                        alert("Kıyaslama yapabilmek için en az 2 araç seçmelisiniz.");
                    }
                }}
            >
                {compareList.length > 1 ? 'Karşılaştırmaya Git ➔' : 'En az 1 araç daha seçin'}
            </button>
        </div>
    );
}