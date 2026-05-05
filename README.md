💧 Arıtma Envanter Yönetim Sistemi
Bu proje, arıtma tesislerindeki envanter akışını, malzeme stoklarını ve kimyasal tüketimlerini dijital ortamda takip etmek için geliştirilmiş modern bir Envanter Yönetim Sistemidir. Web tabanlı yönetim panelinin yanı sıra, mobil uygulama entegrasyonu için hazır bir API mimarisine sahiptir.

 Proje Hakkında
"Arıtma Envanter", karmaşık stok süreçlerini basitleştirmek ve operasyonel verimliliği artırmak amacıyla tasarlanmıştır. Proje, bir envanterin sadece miktarını değil; bulunduğu depoyu, raf bilgisini ve birim türünü (KG/Adet) de kapsayan detaylı bir takip sistemi sunar.

Ana Özellikler
Depo ve Raf Yönetimi: Malzemelerin konum bazlı takibi.

Dinamik Stok Hareketleri: Ürün giriş/çıkış işlemlerinin anlık kaydedilmesi.

Birim Yönetimi: Kimyasal maddeler için KG, parça malzemeler için Adet bazlı hassas takip.

Modern Dashboard: Stok durumlarını ve kritik seviyeleri izlemek için temiz arayüz.

API Desteği: .NET MAUI ve diğer mobil platformlar ile tam senkronizasyon.

🛠️ Kullanılan Teknolojiler
Backend: .NET Core MVC

Veritabanı: PostgreSQL

Frontend: HTML5, CSS3, JavaScript, Bootstrap

ORM: Entity Framework Core

Versiyon Kontrol: Git & GitHub

📁 Veritabanı Mimarisi
Proje, ilişkisel bir veritabanı yapısı üzerine kurulmuştur:

Ürünler (Products): Malzeme bilgileri ve birim türleri.

Depolar (Warehouses): Fiziksel stok alanları.

Stok Hareketleri: Giriş-çıkış geçmişi ve miktar değişimleri.

💻 Kurulum
Projeyi yerel makinenizde çalıştırmak için:
Bu depoyu klonlayın:
- git clone https://github.com/Fatmakkose/Ar-tmaEnvanter.git
- appsettings.json dosyasındaki PostgreSQL bağlantı dizesini (Connection String) kendi bilgilerinizle güncelleyin.

Package Manager Console üzerinden migration'ları uygulayın:
Update-Database
Projeyi çalıştırın.
Geliştirici: Fatma Akköse

İletişim:https://www.linkedin.com/in/fatma-akkose
