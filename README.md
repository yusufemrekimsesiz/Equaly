# Equaly

Arkadaş grupları, ev arkadaşları veya tatil grupları için geliştirilmiş, **grup harcamalarını takip eden ve minimum işlemle borç eşitleyen** bir .NET MAUI mobil uygulaması.

## Özellikler

- 👥 **Kişi Yönetimi** — Gruba kişi ekleme/silme, herkesin net bakiyesini (alacak/borç) canlı takip etme
- 💸 **Harcama Ekleme/Düzenleme/Silme** — Her harcama, dahil edilen katılımcılar arasında otomatik olarak eşit paylaştırılır
- 🎯 **Seçici Katılımcı Desteği** — Bir harcama, gruptaki herkese değil, yalnızca seçtiğin kişilere bölünebilir
- 🤝 **Akıllı Hesaplaşma (Debt Simplification)** — Açgözlü (Greedy) algoritma ile "kim kime ne kadar ödemeli" sorusuna **minimum sayıda transferle** cevap verir
- 🎨 Modern, sade arayüz — pozitif bakiyeler yeşil, negatif bakiyeler kırmızı renkte gösterilir

## Kullanılan Teknolojiler

| Katman | Teknoloji |
|---|---|
| UI Framework | .NET MAUI |
| Mimari | MVVM |
| MVVM Altyapısı | [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) |
| Yerel Veritabanı | [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net) |
| Dependency Injection | .NET yerleşik DI container (`MauiProgram.cs`) |

## Mimari Genel Bakış

```
Models/        → Person, Expense, ExpenseParticipant, Settlement
Services/      → DatabaseService (SQLite CRUD + bakiye hesaplama)
                 SettlementService (Greedy borç eşitleme algoritması)
ViewModels/    → PeopleViewModel, AddExpenseViewModel,
                 ExpensesViewModel, SettlementsViewModel
Views/         → PeoplePage, AddExpensePage,
                 ExpensesPage, SettlementsPage
```

### Bakiye Hesaplama Mantığı

Her harcama eklendiğinde/güncellendiğinde/silindiğinde, tüm bakiyeler sıfırdan yeniden hesaplanır:

- Her harcamanın tutarı, **sadece o harcamaya dahil edilen katılımcılar** arasında eşit bölünür.
- Ödeyen kişi harcamanın tamamı kadar alacaklanır, payı kadar da borçlanır — net etkisi `tutar - kendi payı` kadar alacaklı olmasıdır.

### Greedy Hesaplaşma Algoritması

`SettlementService`, en çok borçlu kişi ile en çok alacaklı kişiyi sırayla eşleştirerek minimum sayıda ödeme işlemi üretir. Her adımda taraflardan en az biri tamamen kapanır, bu sayede sonuç mümkün olan en az transferle elde edilir.

## Kurulum

```bash
git clone https://github.com/yusufemrekimsesiz/Equaly.git
cd Equaly
dotnet restore
```

Visual Studio'da `.sln` dosyasını açıp Android/Windows hedefiyle çalıştırabilir veya:

```bash
dotnet build -t:Run -f net10.0-android
```

komutuyla derleyip çalıştırabilirsiniz.

## Yol Haritası

- [ ] Birden fazla grup desteği (örn. "Ev arkadaşları", "Tatil")
- [ ] Harcamaları tarihe göre filtreleme/gruplama
- [ ] Hesaplaşma ekranından ödemeyi "yapıldı" olarak işaretleme

## Lisans

Kişisel kullanım amaçlı geliştirilmiştir.
