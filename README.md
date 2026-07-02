# 🚗 CarAdvisor - AI-Powered Car Recommendation & Comparison Platform

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Google Gemini](https://img.shields.io/badge/Google%20Gemini-8E75B2?style=for-the-badge&logo=google&logoColor=white)
![Kafka](https://img.shields.io/badge/Apache_Kafka-231F20?style=for-the-badge&logo=apache-kafka&logoColor=white)

**CarAdvisor** is a modern, full-stack web application designed to help users find their dream cars using the power of Artificial Intelligence. Instead of manually filtering through endless lists of specifications, users can simply type what they want in natural language (e.g., *"I'm looking for a fuel-efficient SUV under 1 million TL with a large trunk"*), and the built-in AI assistant will query the database to provide the perfect matches.

---

## ✨ Key Features

- **🧠 AI-Driven Natural Language Search:** Integrated with **Google Gemini** via Semantic Kernel. The AI translates user prompts into complex database queries instantly.
- **📊 Dynamic Car Comparison:** Add up to 4 cars to your comparison context to evaluate specs, fuel consumption, acceleration, and real-time prices side-by-side.
- **⚙️ Automated Data Scraping & Sync:** Uses a robust asynchronous background architecture (**Hangfire + Apache Kafka + n8n**) to scrape real-time car prices from external sources without blocking the main API thread.
- **🎨 Modern UI/UX:** Built with React and Context API, featuring a premium glassmorphism design, smooth micro-animations, and a highly responsive layout.

---

## 💻 Technology Stack

### Backend (Web API)
- **Framework:** .NET 8 (C#)
- **Database ORM:** Entity Framework Core (Code-First Approach)
- **Database Engine:** Microsoft SQL Server
- **AI Integration:** Microsoft Semantic Kernel (Google Gemini `gemini-flash-latest`)
- **Background Jobs & Messaging:** Hangfire, Apache Kafka
- **Mapping:** AutoMapper
- **Architecture:** N-Tier Architecture (API, Business, DataAccess)

### Frontend (User Interface)
- **Library:** React.js (Vite)
- **State Management:** React Context API (e.g., `CompareContext`)
- **Styling:** Custom CSS, Bootstrap
- **Markdown Rendering:** `react-markdown`, `remark-gfm`

### Automation
- **Workflow Automation:** n8n

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- A Google Gemini API Key

### 1. Backend Setup (API)
1. Navigate to the API folder:
   ```bash
   cd CarAdvisor/CarAdvisor.API
   ```
2. Create or update your `appsettings.json` file with your SQL Server connection string and Gemini API Key:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=CarAdvisor;Integrated Security=True;TrustServerCertificate=True;"
     },
     "Gemini": {
       "ApiKey": "YOUR_GEMINI_API_KEY",
       "ModelId": "gemini-flash-latest"
     }
   }
   ```
3. Apply Entity Framework Migrations (if needed) and run the project:
   ```bash
   dotnet build
   dotnet run
   ```
   *The API will start at `http://localhost:5295`.*

### 2. Frontend Setup (UI)
1. Navigate to the UI folder:
   ```bash
   cd car-advisor-ui/car-advisor-ui
   ```
2. Install the dependencies:
   ```bash
   npm install
   ```
3. Start the Vite development server:
   ```bash
   npm run dev
   ```
   *The application will be accessible at `http://localhost:5173`.*

---

## 🏗️ Architecture Highlights

- **Security First:** Sensitive credentials like API keys and Connection Strings are strictly managed via configurations and excluded from version control using `.gitignore`.
- **Decoupled Processing:** Scraping intensive data (like live pricing) pushes messages to an **Apache Kafka** topic. A **Hangfire** worker then processes this queue asynchronously, ensuring zero impact on the end-user API response times.
- **Smart Plugins:** The AI assistant uses a dedicated `CarDatabasePlugin` to interact safely and efficiently with the SQL database, proving that traditional relational databases can be seamlessly married to modern LLMs.

---

> **Note:** This project was developed as a comprehensive full-stack showcase to demonstrate the integration of traditional enterprise architecture (.NET/SQL) with modern AI toolchains and event-driven microservice concepts.
