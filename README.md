# FilmFree-watch-free-movies
Film İzleme Sitesi (http://52.59.124.115:85/)
C# Asp.net Framework’ün MVC mimarisi ile hazırlanan bu sitede MSSQL veritabanı kullanılmıştır.

- Authorize işlemleri

Uygulamada 3 farklı yetki sınıflandırması var. Sınıflandırmaya göre kullanıcı user, admin veya superadmin olarak 
işlem yapabiliyor.
superadmin normal kullanıcılara yetki tanıyabilir, yetkili kullanıcılar ise admin paneline erişebilir.
Superadmin giriş bilgileri -> Kullanıcı Adı: c
 Şifre: c

- Giriş ve Kayıt olma

Uygulama kayıt olan kullanıcılara SMTP servisini kullanarak onay maili yollar. Bu mailde bulunan link ziyaret 
edilmediği sürece kullanıcı giriş yapamaz.
Şifre unutma durumunda ise aynı şekilde mail onayı ile şifre sıfırlanabilmektedir.

- Ajax kullanımı

Uygulamada Ajax ile veri çekme yöntemi çok kez kullanılmıştır.

- Aktif kullanılabilen özellikler

Kullanıcı filmleri beğenebilir, yorum yapabilir(yorum admin tarafından onaylanır ise yayına alınır), film izlemede 
sorun yaşanırsa hata bildirebilir, iletişime geç ile yöneticilere mesaj gönderebilir, türlere göre filmleri filtreleyebilir veya 
film ismi, oyuncu arayarak filmlere erişebilir.
 
-Admin paneli işlemleri

Yorumlar: Kullanıcıların yaptığı yorumlar onaylanabilir.
İstatistik: Sitede bulunan filmlerin sayısı, yapılan yorumların sayısı gibi bilgileri grafik biçiminde görebilir.
Film Yükle: Siteye film yükleyebilir.
Filme Link Ekle: Eklenen filmlere alternatif kaynaklar ekleyebilir veya var olanları düzenleyebilir.
İletişim Formu: İletişim formu gönderen kullanıcıların mesajlarını okuyabilir.
Bildirilen Hatalar: İzleyicilerin bildirdiği hataları görüntüleyebilir ve müdahale edebilir.
Anasayfa Slayt Ayarları: Anasayfada bulunan slaytı güncel tutmak için düzenleyebilir.
Filme Erişimi Kapat: Filmlere erişimi kısıtlayabilir veya kısıtlanan filmleri erişime açabilir.
Kullanıcı Engelle: Kullanıcıları engelleyebilir.
Tür Ayarları: Film türlerini düzenleyebilir.
