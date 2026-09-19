# AmazonClone — Microservices (.NET 9)

A microservices-based e-commerce backend built with **.NET 9**, **Clean Architecture**, **EF Core + PostgreSQL**, **Redis**, and **Ocelot API Gateway**.

## 🏗️ Architecture

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Docker + Docker Compose

### 1. Start infrastructure
```bash
docker compose up -d
sleep 15
docker exec amazonclone-postgres psql -U postgres -c "CREATE DATABASE catalog_db;"
docker exec amazonclone-postgres psql -U postgres -c "CREATE DATABASE identity_db;"
docker exec amazonclone-postgres psql -U postgres -c "CREATE DATABASE orders_db;"
Apply migratins
dotnet ef database update --project src/Services/Catalog/Catalog.Infrastructure --startup-project src/Services/Catalog/Catalog.Api
dotnet ef database update --project src/Services/Identity/Identity.Infrastructure --startup-project src/Services/Identity/Identity.Api
dotnet ef database update --project src/Services/Orders/Orders.Infrastructure --startup-project src/Services/Orders/Orders.Api
Run all service 
bash ~/stop-all.sh




🔐 Authentication
JWT-based. Get token from Identity:

bash
curl -X POST http://localhost:5002/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"Test123","firstName":"Ali","lastName":"Ahmadi"}'
Use token:

text
Authorization: Bearer <token>
🧪 End-to-End Test
bash
# Login
TOKEN=$(curl -s -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@test.com","password":"Test123"}' | jq -r .accessToken)

# Create category
CATEGORY_ID=$(curl -s -X POST http://localhost:5001/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"Phones","slug":"phones"}' | jq -r .id)

# Create product
PRODUCT_ID=$(curl -s -X POST http://localhost:5001/api/products \
  -H "Content-Type: application/json" \
  -d "{\"categoryId\":\"$CATEGORY_ID\",\"name\":\"iPhone 15\",\"slug\":\"iphone-15\",\"price\":999.99,\"currency\":\"USD\"}" | jq -r .id)

# Add to basket
curl -X POST http://localhost:5004/api/basket/items \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"productId\":\"$PRODUCT_ID\",\"productName\":\"iPhone 15\",\"unitPrice\":999.99,\"quantity\":2}"

# Create order
curl -X POST http://localhost:5003/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"items\":[{\"productId\":\"$PRODUCT_ID\",\"productName\":\"iPhone 15\",\"unitPrice\":999.99,\"quantity\":2}],\"address\":{\"street\":\"123 Main\",\"city\":\"Tehran\",\"country\":\"Iran\"},\"currency\":\"USD\"}"
🏛️ Project Structure
text
src/
├── ApiGateway/Gateway.Api              # Ocelot API Gateway
├── Services/
│   ├── Catalog/                        # Products + Categories
│   │   ├── Catalog.Domain              # Entities
│   │   ├── Catalog.Application         # DTOs + Validators
│   │   ├── Catalog.Infrastructure      # EF Core + DbContext
│   │   └── Catalog.Api                 # Controllers + Program
│   ├── Identity/                       # Users + Roles + JWT
│   ├── Orders/                         # Orders + Items + Payments
│   └── Basket/                         # Redis-based shopping cart
└── BuildingBlocks/                     # Shared utilities
🛠️ Tech Stack
.NET 9 — ASP.NET Core Web API

EF Core 9 + Npgsql — PostgreSQL ORM

FluentValidation — Input validation

JWT — Authentication

Redis — Basket storage

Ocelot — API Gateway

PostgreSQL 16 — Database

Docker — Infrastructure

📝 License
MIT

text

---

## ۴. پاک کردن فایل‌های زائد

```bash
cd ~/Downloads/amazon-clone

# پاک کردن Migration های تکراری (اگه هست)
ls src/Services/*/*/Migrations/ 2>/dev/null

# پاک کردن فایل‌های موقت
rm -rf /tmp/gen_*.sh
rm -rf /tmp/cat_setup.sh
۵. تست نهایی — کل flow از Gateway
bash
# از طریق Gateway (پورت 5000) — نقطه ورود واحد

# ۱. ثبت‌نام
curl -s -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"final@test.com","password":"Test123","firstName":"Final","lastName":"Test"}' | jq

# ۲. لاگین
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"final@test.com","password":"Test123"}' | jq -r .accessToken)

# ۳. محصولات
curl -s http://localhost:5000/api/products | jq

# ۴. سبد
curl -s http://localhost:5000/api/basket -H "Authorization: Bearer $TOKEN" | jq

# ۵. سفارش‌ها
curl -s http://localhost:5000/api/orders -H "Authorization: Bearer $TOKEN" | jq
🎯 چک‌لیست نهایی
کار	وضعیت
PostgreSQL + Redis	✅
Catalog (Products + Categories)	✅
Identity (Users + JWT)	✅
Orders (Orders + Payments)	✅
Basket (Redis)	✅
Gateway (Ocelot)	✅
Migrations	✅
End-to-End Test	✅
README	✅
.gitignore	✅
docker-compose	✅

