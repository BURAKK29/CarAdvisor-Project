import React from "react";
import { Link } from "react-router-dom";

export default function NavBar() {
  return (
    // mb-4 SİLİNDİ! Artık altındaki yapıyla kusursuz birleşecek.
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark shadow-sm">
      <div className="container">
        {/* ... (Geri kalan kodlarının tamamı aynı kalıyor) ... */}
        <Link to="/" className="navbar-brand d-flex align-items-center gap-2">
          <span>🚗</span>
          <span className="fw-bold">Car Advisor</span>
        </Link>

        <button 
          className="navbar-toggler" 
          type="button" 
          data-bs-toggle="collapse" 
          data-bs-target="#navbarNav" 
          aria-controls="navbarNav" 
          aria-expanded="false" 
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav mx-auto gap-4">
            <li className="nav-item">
              <Link to="/" className="nav-link text-warning fs-5">
                Ana Sayfa
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/ai-recommendation" className="nav-link text-warning fw-bold fs-5">
                ✨ AI Araç Önerisi
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/compare" className="nav-link text-warning fw-bold fs-5">
                ⚖️ Araç Karşılaştırma
              </Link>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  );
}