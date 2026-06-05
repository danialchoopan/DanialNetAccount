# DanialNet Account 🚀

**DanialNet Account** is a high-performance, enterprise-grade accounting and financial management system designed for Small and Medium Enterprises (SMEs). Built with **ASP.NET Core 10.0 MVC**, **Entity Framework Core**, and **Tailwind CSS**, it offers a robust solution for double-entry bookkeeping, inventory management, and financial reporting.

---

## ✨ Key Features

### 1. 📖 Double-Entry Ledger System
The heart of the system is a strict double-entry ledger. Every financial transaction is validated to ensure that **Total Debits = Total Credits**.
- **Dynamic Journaling**: Add multiple rows to a journal entry with real-time balance checking via JavaScript.
- **ACID Compliance**: Transactions are handled using database transactions to ensure data integrity.

### 2. 🌳 Hierarchical Chart of Accounts
Manage your accounts with a multi-level tree structure.
- **Self-Referencing Model**: Accounts can have parent-child relationships (e.g., Assets -> Current Assets -> Bank).
- **Interactive TreeView**: Explore your financial structure with a clean, collapsible interface.

### 3. 📦 Advanced Inventory Engine
A sophisticated engine to track stock levels and calculate costs.
- **Strategy Pattern**: Choose between **FIFO** (First-In-First-Out) or **Weighted Average** costing methods.
- **Negative Stock Protection**: The system prevents sales if inventory is insufficient, maintaining logic integrity.

### 4. 🧾 Dynamic Invoicing & Sales Integration
Streamlined sales workflow integrated with the ledger and inventory.
- **Real-time Tax & Totals**: Automatically calculates VAT (9%) and totals as you add items.
- **Automatic Posting**: Saving an invoice automatically reduces stock, records COGS, and updates the General Ledger.
- **Voiding Logic**: Properly reverse invoices with a single click, restoring stock and creating reversing journal entries.

### 5. 📊 Financial Intelligence Dashboard
Get instant insights into your company's health.
- **Real-time Reports**: Generate **Trial Balance**, **Profit & Loss**, and **Balance Sheet** statements.
- **IMemoryCache**: High-performance reporting using intelligent caching.
- **Account Ledger**: Drill down into any account to see a chronological list of transactions and a running balance.

---

## 🛠 Technical Implementation

### Architecture
- **MVC Pattern**: Clear separation of concerns between Models, Views, and Controllers.
- **Service Layer**: Business logic (Inventory, Reports) is encapsulated in dedicated service classes.
- **Tailwind CSS & Vazirmatn**: A modern, responsive UI built with a premium Iranian font face.

### Folder Structure
- `Controllers/`: Handles user requests and orchestrates business flow.
- `Data/`: Contains `ApplicationDbContext` and `DbInitializer` for seeding.
- `Models/`: Core domain entities (Accounts, JournalEntries, Products, Invoices).
- `Services/`: Business logic services (Strategy Pattern for Inventory, Reporting logic).
- `ViewModels/`: Data transfer objects optimized for the Views.
- `Views/`: Razor-based UI templates styled with Tailwind CSS.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker (Optional)

### Run Locally
1. Clone the repository.
2. Run `dotnet run`.
3. Open `http://localhost:5000` (or the port specified in console).
4. The database will automatically be created and seeded with sample data.

### Run with Docker
```bash
docker-compose up --build
```

---

## 📸 System Previews
*Operational views showing seeded enterprise data.*

1. **Dashboard Overview**: Financial health at a glance.
2. **Chart of Accounts**: Comprehensive hierarchical structure.
3. **Invoice Management**: Modern invoice generation with status tracking.
4. **Profit & Loss Statement**: Deep dive into revenue and expenses.

*(Screenshots available in the `docs/screenshots` folder)*

---
Developed with ❤️ by DanialNet Team.
