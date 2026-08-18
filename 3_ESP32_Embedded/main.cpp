#include <Arduino.h>
#include <WiFi.h>
#include <HTTPClient.h>
#include <ArduinoJson.h>
#include <DHT.h>

const char* agAdi = "WIFI_SSID";
const char* sifre = "WIFI_PASSWORD";
// Python sunucunuzun (FastAPI) çalıştığı yerel IP adresi
const char* sunucuAdresi = "http://192.168.1.X:5000/api/sera";

#define DHT_PIN 4
#define DHT_TIPI DHT22 // Sensörünüz DHT11 ise burayı DHT11 olarak değiştirebiliriz
DHT dht(DHT_PIN, DHT_TIPI);

const int toprakNemiPini = 34; // Analog pin - Toprak Nemi
const int isikSensoruPini = 35; // Analog pin - Işık

// Röle Pinleri
const int pompaRolesi = 25;
const int fanRolesi = 26;
const int aydinlatmaRolesi = 27;

void setup() {
  Serial.begin(115200);
  
  pinMode(pompaRolesi, OUTPUT);
  pinMode(fanRolesi, OUTPUT);
  pinMode(aydinlatmaRolesi, OUTPUT);
  
  // Güvenlik amaçlı röleleri kapalı başlat (Röle modülünüz LOW/HIGH tetiklemeli olabilir, buna göre ayarlayın)
  digitalWrite(pompaRolesi, LOW);
  digitalWrite(fanRolesi, LOW);
  digitalWrite(aydinlatmaRolesi, LOW);

  dht.begin();
  
  WiFi.begin(agAdi, sifre);
  Serial.print("WiFi Baglaniliyor");
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nWiFi Baglandi!");
}

void loop() {
  if(WiFi.status() == WL_CONNECTED){
    HTTPClient http;
    http.begin(sunucuAdresi);
    http.addHeader("Content-Type", "application/json");

    float nem = dht.readHumidity();
    float sicaklik = dht.readTemperature();
    int toprakNemi = analogRead(toprakNemiPini);
    int isik = analogRead(isikSensoruPini);

    if (isnan(nem) || isnan(sicaklik)) {
      Serial.println("DHT okuma hatasi!");
      return;
    }

    // Python sunucusuna gönderilecek JSON verisi
    StaticJsonDocument<200> jsonBelgesi;
    jsonBelgesi["sicaklik"] = sicaklik;
    jsonBelgesi["nem"] = nem;
    jsonBelgesi["toprak_nemi"] = toprakNemi;
    jsonBelgesi["isik"] = isik;

    String istekGovdesi;
    serializeJson(jsonBelgesi, istekGovdesi);

    // REST API'ye POST isteği atıyoruz
    int httpYanitKodu = http.POST(istekGovdesi);

    if (httpYanitKodu > 0) {
      String yanit = http.getString();
      StaticJsonDocument<200> yanitBelgesi;
      deserializeJson(yanitBelgesi, yanit);

      // Python'daki algoritmadan dönen komutlar
      bool pompaDurumu = yanitBelgesi["su_pompasi"];
      bool fanDurumu = yanitBelgesi["fan"];
      bool aydinlatmaDurumu = yanitBelgesi["aydinlatma"];

      digitalWrite(pompaRolesi, pompaDurumu ? HIGH : LOW);
      digitalWrite(fanRolesi, fanDurumu ? HIGH : LOW);
      digitalWrite(aydinlatmaRolesi, aydinlatmaDurumu ? HIGH : LOW);
      
      Serial.println("Guncel Durum Alindi ve Uygulandi.");
    } else {
      Serial.print("HTTP Hata Kodu: ");
      Serial.println(httpYanitKodu);
    }
    http.end();
  }
  
  // 5 saniyede bir döngü
  delay(5000);
}