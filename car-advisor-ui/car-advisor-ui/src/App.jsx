import 'bootstrap/dist/css/bootstrap.min.css'; 
import { useState } from "react";
import HomePage from './pages/HomePage';
import { Routes, Route, Router } from 'react-router-dom';
import BrandModelsPages from './pages/BrandModelsPage';
import ChatWidget from './components/ChatWidget'; // <-- BUNU EKLE
import AiRecommendationPage from './pages/AiRecommendationPage';
import NavBar from './components/NavBar';
import ModelDetailsPage from './pages/ModelDetailsPage';
import TrimPackagesPage from './pages/TrimPackagesPage';
import BodyTypeModelsPage from './pages/BodyTypeModelsPage';
import { CompareProvider } from './context/CompareContext';
import CompareWidget from './components/CompareWidget';
import ComparePage from './pages/ComparePage';
function App()
{
  return (
    <div>
      <CompareProvider>
      <NavBar/>
      <Routes>
        {/* Ana Sayfa (/) açılınca BrandList gelsin */}
        <Route path="/" element={<HomePage />} />
        
        <Route path="/model/:brandName" element={<BrandModelsPages />} /> 
        <Route path="/ai-recommendation" element={<AiRecommendationPage />} />  
        <Route path="/details/:brandName/:modelName/:generationName/:bodyType?" element={<TrimPackagesPage />} />        
        <Route path="/trim-details" element={<ModelDetailsPage />} />
        <Route path="/body-type/:bodyTypeName" element={<BodyTypeModelsPage />} />
        <Route path="/compare" element={<ComparePage />} />
    </Routes> 
      <ChatWidget/>
      <CompareWidget/>
      </CompareProvider>
    </div>
  )
}
export default App