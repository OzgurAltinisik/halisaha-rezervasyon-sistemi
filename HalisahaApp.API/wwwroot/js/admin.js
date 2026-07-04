const API = 'http://localhost:5156';
let token = localStorage.getItem('adminToken');
let sahaId = parseInt(localStorage.getItem('sahaId') || '0');

// Sayfa yüklenince oturum kontrolü
window.onload = function () {
    if (token && sahaId) {
        panelGoster();
    }
    // Bugünün tarihini filtre alanına yaz
    const bugun = new Date().toISOString().split('T')[0];
    const filterTarih = document.getElementById('filterTarih');
    const manTarih = document.getElementById('manTarih');
    if (filterTarih) filterTarih.value = bugun;
    if (manTarih) manTarih.value = bugun;
};

// GİRİŞ
async function girisYap() {
    const email = document.getElementById('loginEmail').value.trim();
    const sifre = document.getElementById('loginSifre').value;
    const hata = document.getElementById('loginHata');
    hata.style.display = 'none';

    try {
        const res = await fetch(`${API}/api/auth/giris`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, sifre })
        });

        const data = await res.json();

        if (res.ok) {
            token = data.token;
            localStorage.setItem('adminToken', token);

            if (data.sahalar && data.sahalar.length > 0) {
                sahaId = data.sahalar[0].id;
                localStorage.setItem('sahaId', sahaId);
                document.getElementById('sidebarSahaAdi').textContent = data.sahalar[0].ad;
            }
            panelGoster();
        } else {
            hata.textContent = data.mesaj || 'Giriş başarısız.';
            hata.style.display = 'block';
        }
    } catch {
        hata.textContent = 'Sunucuya bağlanılamadı.';
        hata.style.display = 'block';
    }
}

function panelGoster() {
    document.getElementById('loginWrap').style.display = 'none';
    document.getElementById('adminWrap').style.display = 'grid';
    istatistikYukle();
    dashRezYukle();
}

function cikisYap() {
    localStorage.removeItem('adminToken');
    localStorage.removeItem('sahaId');
    location.reload();
}

// SAYFA GEÇİŞİ
function sayfaGoster(sayfa) {
    document.querySelectorAll('.admin-page').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.sidebar-link').forEach(l => l.classList.remove('active'));
    document.getElementById('page-' + sayfa).classList.add('active');
    event.target.classList.add('active');

    if (sayfa === 'dashboard') {
        istatistikYukle();
        dashRezYukle();
    }
    if (sayfa === 'ayarlar') ayarlariYukle();
}

// İSTATİSTİK
async function istatistikYukle() {
    try {
        const res = await fetch(`${API}/api/admin/istatistik?sahaId=${sahaId}`, {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (!res.ok) return;
        const data = await res.json();
        document.getElementById('statBugunRez').textContent = data.bugunRezervasyonSayisi;
        document.getElementById('statBugunGelir').textContent = data.bugunGelir.toLocaleString('tr-TR') + ' ₺';
        document.getElementById('statHaftaGelir').textContent = data.haftaGelir.toLocaleString('tr-TR') + ' ₺';
    } catch { }
}

// DASHBOARD - BUGÜNKÜ REZ
async function dashRezYukle() {
    const bugun = new Date().toISOString().split('T')[0];
    const list = document.getElementById('dashRezList');
    list.innerHTML = '<div class="empty-state">Yükleniyor...</div>';
    try {
        const res = await fetch(`${API}/api/admin/rezervasyonlar?sahaId=${sahaId}&tarih=${bugun}`, {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        const data = await res.json();
        renderRezList(data, list, false);
    } catch {
        list.innerHTML = '<div class="empty-state">Yüklenemedi.</div>';
    }
}

// REZERVASYONLAR SAYFASI
async function rezervasyonlariYukle() {
    const tarih = document.getElementById('filterTarih').value;
    const list = document.getElementById('rezList');
    list.innerHTML = '<div class="empty-state">Yükleniyor...</div>';
    try {
        const res = await fetch(`${API}/api/admin/rezervasyonlar?sahaId=${sahaId}&tarih=${tarih}`, {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        const data = await res.json();
        renderRezList(data, list, true);
    } catch {
        list.innerHTML = '<div class="empty-state">Yüklenemedi.</div>';
    }
}

function renderRezList(liste, container, iptalButonu) {
    if (!liste || liste.length === 0) {
        container.innerHTML = '<div class="empty-state">Bu tarihte rezervasyon yok.</div>';
        return;
    }
    container.innerHTML = '';
    liste.forEach(r => {
        const div = document.createElement('div');
        div.className = 'rez-item';
        const odeme = r.odemeDurumu === 'Odendi' ? '<span class="badge badge-paid">Ödendi</span>' : '<span class="badge badge-pending">Bekliyor</span>';
        const iptal = iptalButonu && r.durum === 'Aktif'
            ? `<button class="btn-sm btn-red" onclick="iptalEt(${r.id})">İptal</button>` : '';

        div.innerHTML = `
      <div class="rez-time">${r.tarihVeSaat ? r.tarihVeSaat.split(' ').slice(3).join(' ') : '—'}</div>
      <div class="rez-info">
        <div class="rez-name">${r.adSoyad}</div>
        <div class="rez-detail">${r.email} · ${r.toplamUcret.toLocaleString('tr-TR')} ₺</div>
      </div>
      <div class="rez-actions">
        ${odeme}
        ${iptal}
      </div>
    `;
        container.appendChild(div);
    });
}

// İPTAL
async function iptalEt(id) {
    if (!confirm('Bu rezervasyonu iptal etmek istediğinize emin misiniz?')) return;
    try {
        const res = await fetch(`${API}/api/admin/iptal/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ neden: 'Admin tarafından iptal edildi.' })
        });
        if (res.ok) {
            alert('Rezervasyon iptal edildi.');
            rezervasyonlariYukle();
            istatistikYukle();
        }
    } catch { alert('Hata oluştu.'); }
}

// MANUEL EKLE
async function manuelEkle() {
    const adSoyad = document.getElementById('manAdSoyad').value.trim();
    const tarih = document.getElementById('manTarih').value;
    const saat = parseInt(document.getElementById('manSaat').value);
    const msg = document.getElementById('manuelMsg');

    if (!adSoyad || !tarih) {
        msg.textContent = 'Ad soyad ve tarih zorunludur.';
        msg.style.color = '#c0392b';
        msg.style.display = 'block';
        return;
    }

    try {
        const res = await fetch(`${API}/api/admin/manuel-rezervasyon`, {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ sahaId, adSoyad, tarih, baslangicSaati: saat })
        });
        const data = await res.json();
        if (res.ok) {
            msg.textContent = '✅ Rezervasyon eklendi.';
            msg.style.color = '#1a7a3c';
            document.getElementById('manAdSoyad').value = '';
        } else {
            msg.textContent = data.mesaj || data.title || 'Hata oluştu.';
            msg.style.color = '#c0392b';
        }
        msg.style.display = 'block';
    } catch {
        msg.textContent = 'Sunucuya bağlanılamadı.';
        msg.style.color = '#c0392b';
        msg.style.display = 'block';
    }
}

// AYARLAR YÜKLEindeki
async function ayarlariYukle() {
    // Şimdilik mevcut saha bilgisi için ayrı endpoint ekleyebiliriz
    // Şimdilik sadece boş form göster
}

// AYARLAR KAYDET
async function ayarlariKaydet() {
    const msg = document.getElementById('ayarMsg');
    const body = {
        ad: document.getElementById('ayarAd').value,
        acilisSaati: parseInt(document.getElementById('ayarAcilis').value),
        kapanisSaati: parseInt(document.getElementById('ayarKapanis').value),
        saatlikUcret: parseFloat(document.getElementById('ayarUcret').value)
    };

    try {
        const res = await fetch(`${API}/api/admin/saha-ayarlari/${sahaId}`, {
            method: 'PUT',
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body)
        });
        const data = await res.json();
        if (res.ok) {
            msg.textContent = '✅ Ayarlar kaydedildi.';
            msg.style.color = '#1a7a3c';
        } else {
            msg.textContent = 'Hata: ' + (data.mesaj || 'Kaydedilemedi.');
            msg.style.color = '#c0392b';
        }
        msg.style.display = 'block';
    } catch {
        msg.textContent = 'Sunucuya bağlanılamadı.';
        msg.style.color = '#c0392b';
        msg.style.display = 'block';
    }
}