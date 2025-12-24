# VPS Direct Deployment - No Containers

## FASTEST VPS DEPLOYMENT

### Prerequisites
- VPS with Ubuntu/Debian
- SSH access
- Domain/IP address

---

## 1. Install .NET on VPS (5 mins)

```bash
# SSH into VPS
ssh user@your-vps-ip

# Install .NET 8
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Add to PATH
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify
dotnet --version
```

---

## 2. Install PostgreSQL (or use SQLite)

### Option A: PostgreSQL
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib -y

# Create database
sudo -u postgres psql
CREATE DATABASE clinic_db;
CREATE USER clinic_admin WITH PASSWORD 'YourPassword123!';
GRANT ALL PRIVILEGES ON DATABASE clinic_db TO clinic_admin;
\q
```

### Option B: SQLite (FASTEST for tomorrow)
```bash
# Nothing to install! Just use SQLite
```

---

## 3. Deploy API to VPS

### On your local machine:
```bash
# Build and publish
cd /path/to/ClinicAPI
dotnet publish -c Release -o ./publish

# Copy to VPS (replace with your details)
scp -r ./publish/* user@your-vps-ip:/home/user/clinic-api/
```

### On VPS:
```bash
cd /home/user/clinic-api

# Update appsettings.json with production settings
nano appsettings.json
```

### appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=clinic.db"
  },
  "JWT": {
    "Secret": "YourSuperSecretKeyMinimum32Characters!!",
    "Issuer": "ClinicAPI",
    "Audience": "ClinicApp",
    "ExpirationHours": 24
  },
  "AllowedHosts": "*",
  "Urls": "http://0.0.0.0:5000"
}
```

---

## 4. Run API as a Service

### Create systemd service:
```bash
sudo nano /etc/systemd/system/clinic-api.service
```

### Service file content:
```ini
[Unit]
Description=Clinic API
After=network.target

[Service]
WorkingDirectory=/home/user/clinic-api
ExecStart=/home/user/.dotnet/dotnet /home/user/clinic-api/ClinicAPI.dll
Restart=always
RestartSec=10
SyslogIdentifier=clinic-api
User=user
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

### Start service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable clinic-api
sudo systemctl start clinic-api
sudo systemctl status clinic-api

# View logs
journalctl -u clinic-api -f
```

---

## 5. Setup Firewall

```bash
# Allow API port
sudo ufw allow 5000/tcp

# Allow SSH (if not already)
sudo ufw allow 22/tcp

# Enable firewall
sudo ufw enable
```

---

## 6. Setup Nginx (Optional - for HTTPS)

```bash
sudo apt install nginx -y

sudo nano /etc/nginx/sites-available/clinic-api
```

### Nginx config:
```nginx
server {
    listen 80;
    server_name your-domain.com;  # or use IP

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/clinic-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

---

## 7. Android App Configuration

### Update API URL in Android:
```java
// In RetrofitClient.java or Constants.java
public static final String BASE_URL = "http://your-vps-ip:5000/api/";
// OR with nginx:
public static final String BASE_URL = "http://your-domain.com/api/";
```

---

## 8. Quick Deployment Script

### Create `deploy.sh` on local machine:
```bash
#!/bin/bash

echo "Building API..."
dotnet publish -c Release -o ./publish

echo "Deploying to VPS..."
scp -r ./publish/* user@your-vps-ip:/home/user/clinic-api/

echo "Restarting service..."
ssh user@your-vps-ip "sudo systemctl restart clinic-api"

echo "Deployment complete!"
echo "Check status: ssh user@your-vps-ip 'sudo systemctl status clinic-api'"
```

```bash
chmod +x deploy.sh
./deploy.sh
```

---

## 9. Testing

### From your machine:
```bash
# Health check
curl http://your-vps-ip:5000/api/health

# Test login
curl -X POST http://your-vps-ip:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"clerk@clinic.com","password":"Password123!"}'
```

### From Android phone:
1. Install APK on 3 phones
2. Make sure phones have internet (WiFi/4G)
3. Test login on each phone with different roles

---

## 10. Troubleshooting

### API not starting:
```bash
# Check logs
journalctl -u clinic-api -n 50

# Check if port is in use
sudo netstat -tlnp | grep 5000

# Run manually to see errors
cd /home/user/clinic-api
dotnet ClinicAPI.dll
```

### Can't connect from phone:
```bash
# Check firewall
sudo ufw status

# Check if API is listening
curl http://localhost:5000/api/health

# Check from external
curl http://your-vps-ip:5000/api/health
```

### Database errors:
```bash
# For SQLite - create database
cd /home/user/clinic-api
dotnet ef database update

# Check database file permissions
ls -la clinic.db
chmod 664 clinic.db
```

---

## ONE-LINER DEPLOYMENT (after initial setup)

```bash
dotnet publish -c Release -o ./publish && \
scp -r ./publish/* user@vps-ip:/home/user/clinic-api/ && \
ssh user@vps-ip "sudo systemctl restart clinic-api"
```

---

## Environment Variables (for secrets)

### Option 1: In systemd service file
```ini
[Service]
Environment=JWT__Secret=YourSecretKey
Environment=ConnectionStrings__DefaultConnection=YourConnectionString
```

### Option 2: In .env file
```bash
# Create .env
nano /home/user/clinic-api/.env
```

```bash
JWT__Secret=YourSecretKey
ConnectionStrings__DefaultConnection=Data Source=clinic.db
```

Update service to load .env:
```ini
[Service]
EnvironmentFile=/home/user/clinic-api/.env
```

---

## CHECKLIST FOR TOMORROW

- [ ] VPS accessible
- [ ] .NET 8 installed on VPS
- [ ] API code published
- [ ] systemd service created and running
- [ ] Firewall configured (port 5000 open)
- [ ] Database created/migrations run
- [ ] Seed data loaded
- [ ] Android app updated with VPS IP
- [ ] Tested from all 3 phones
- [ ] Demo script prepared

**Estimated time: 1-2 hours**
