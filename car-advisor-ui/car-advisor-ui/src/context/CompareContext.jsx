import React, { createContext, useState, useEffect, useContext } from 'react';

// Context'i oluşturuyoruz
const CompareContext = createContext();

// Diğer sayfalarda kolayca kullanabilmek için özel bir Hook yazıyoruz
export const useCompare = () => useContext(CompareContext);

export const CompareProvider = ({ children }) => {
    // Sayfa ilk yüklendiğinde localStorage'da araç var mı diye bakıyoruz
    const [compareList, setCompareList] = useState(() => {
        const saved = localStorage.getItem('car_compare_list');
        return saved ? JSON.parse(saved) : [];
    });

    // Liste her değiştiğinde localStorage'ı güncelliyoruz (Sayfa yenilense bile silinmez)
    useEffect(() => {
        localStorage.setItem('car_compare_list', JSON.stringify(compareList));
    }, [compareList]);

    // Sepete Araç Ekleme Fonksiyonu
    const addToCompare = (car) => {
        if (compareList.length >= 4) {
            alert("En fazla 4 araç kıyaslayabilirsiniz!");
            return false; // Eklenemedi
        }
        
        // Araç zaten listede var mı kontrolü
        const isExist = compareList.find(c => 
            c.make === car.make && 
            c.model === car.model && 
            c.generation === car.generation && 
            c.bodyType === car.bodyType
        );

        if (isExist) {
            alert("Bu araç zaten kıyaslama listesinde!");
            return false;
        }

        setCompareList([...compareList, car]);
        return true; // Başarıyla eklendi
    };

    // Sepetten Araç Çıkarma Fonksiyonu
    const removeFromCompare = (carToRemove) => {
        const newList = compareList.filter(c => 
            !(c.make === carToRemove.make && 
              c.model === carToRemove.model && 
              c.generation === carToRemove.generation && 
              c.bodyType === carToRemove.bodyType)
        );
        setCompareList(newList);
    };

    // Sepeti Tamamen Boşaltma
    const clearCompare = () => {
        setCompareList([]);
    };

    return (
        <CompareContext.Provider value={{ compareList, addToCompare, removeFromCompare, clearCompare }}>
            {children}
        </CompareContext.Provider>
    );
};