---
trigger: always_on
---

# Proje Genel Tanımı
Bu proje, kullanıcıların hayalindeki araçları bulup kıyaslayabildiği yapay zeka destekli bir araç öneri ve bilgi sistemidir. 

# Backend Kuralları (C# .NET)
- Çatı: C# .NET Core Web API.
- Veritabanı: Entity Framework Core (SQL Server).
- Asenkron İşlemler & Mesajlaşma: Arka planda fiyat ve görsel çekme işlemleri için Apache Kafka ve Hangfire kullanılmaktadır.
- Yapay Zeka Entegrasyonu: Semantic Kernel kullanılmaktadır.
- Standart: Kodlar yazılırken Controller'lar şişirilmemeli, temiz ve okunabilir bir mimari izlenmelidir.

# Frontend Kuralları (React)
- Çatı: React.js.
- Durum Yönetimi: Kıyaslama sepeti gibi yapılar için Context API (`CompareContext.jsx`) kullanılmaktadır.
- Stil ve Tasarım: Ağırlıklı olarak Bootstrap class'ları ve özel CSS dosyaları kullanılmaktadır. 
- Standart: Yeni bir bileşen (Component) yazılırken mevcut modern ve premium tasarım diline (yuvarlak hatlar, gölgeler, temiz UI) sadık kalınmalıdır.

# Otomasyon
- Dış veri çekme ve iş akışları için n8n kullanılmaktadır.