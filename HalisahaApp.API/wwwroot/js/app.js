const API = 'http://localhost:5156';
const ADMIN_ID = 2;

let sahalar = [];
let secilenTarih = null;
let secilenSaha = null;
let secilenSaat = null;
let saatlikUcret = 0;

const GUNLER = ['Paz', 'Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt'];
const AYLAR = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'];
const AYLAR_UZUN = ['Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'];

window.onload = async function () {
    gunlerOlustur();
    await sahalariVeTabloYukle();
};

function gunlerOlustur() {
    const bar = document.getElementById('dayBar');
    bar.innerHTML = '';
    for (let i = 0; i < 7; i++) {
        const t = new Date();
        t.setDate(t.getDate() + i);
        const btn = document.createElement('button');
        btn.className = 'day-btn' + (i === 0 ? ' active' : '');
        btn.innerHTML = `<span class="dname">${GUNLER[t.getDay()]}</span><span class="dnum">${t.getDate()} ${AYLAR[t.getMonth()]}</span>`;
        btn.dataset.tarih = fmt(t);
        btn.onclick = () => gunSec(btn, fmt(t));
        bar.appendChild(btn);
    }
    secilenTarih = fmt(new Date());
}

function gunSec(btn, tarih) {
    document.querySelectorAll('.day-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    secilenTarih = tarih;
    tabloYukle();
}

async function sahalariVeTabloYukle() {
    try {
        const res = await fetch(`${API}/api/rezervasyon/sahalar?adminId=${ADMIN_ID}`);
        sahalar = await res.json();
        if (sahalar.length > 0) {
            document.getElementById('sahaAdi').textContent = sahalar.length > 1
                ? 'HALISAHA' : sahalar[0].ad;
        }
    } catch (e) {
        console.error('Sahalar yüklenemedi', e);
    }
    await tabloYukle();
}

async function tabloYukle() {
    const container = document.getElementById('tableContainer');
    container.innerHTML = '<div class="spinner"></div><div class="loading-msg">Yükleniyor...</div>';

    if (!sahalar.length) {
        container.innerHTML = '<div class="loading-msg">Saha bulunamadı.</div>';
        return;
    }

    // Tüm sahaların müsaitlik verisini paralel çek
    const veriler = await Promise.all(
        sahalar.map(s =>
            fetch(`${API}/api/rezervasyon/musaitlik?sahaKodu=${s.sahaKodu}&tarih=${secilenTarih}`)
                .then(r => r.ok ? r.json() : null)
                .catch(() => null)
        )
    );

    // Saat aralığını belirle (tüm sahaların en geniş aralığı)
    let minSaat = 17, maxSaat = 24;
    veriler.forEach(v => {
        if (!v) return;
        if (v.saatler && v.saatler.length > 0) {
            minSaat = Math.min(minSaat, v.saatler[0].baslangicSaati);
            maxSaat = Math.max(maxSaat, v.saatler[v.saatler.length - 1].bitisSaati);
        }
    });

    const simdikiSaat = new Date().getHours();
    const bugun = fmt(new Date()) === secilenTarih;

    // Tablo oluştur
    let html = '<table><thead><tr>';
    html += '<th class="th-saat">SAAT</th>';
    sahalar.forEach((s, i) => {
        html += `<th class="th-saha">${s.ad}</th>`;
    });
    html += '</tr></thead><tbody>';

    for (let saat = minSaat; saat < maxSaat; saat++) {
        const saatStr = `${String(saat).padStart(2, '0')}:00–${String(saat + 1).padStart(2, '0')}:00`;
        const gecmis = bugun && saat < simdikiSaat;

        html += `<tr><td class="td-saat">${saatStr}</td>`;

        sahalar.forEach((s, i) => {
            const v = veriler[i];
            let slotHtml = '';

            if (!v) {
                slotHtml = `<div class="slot past">—</div>`;
            } else if (gecmis) {
                slotHtml = `<div class="slot past">Geçmiş Saat</div>`;
            } else {
                const slot = v.saatler ? v.saatler.find(sl => sl.baslangicSaati === saat) : null;
                if (!slot) {
                    slotHtml = `<div class="slot past">—</div>`;
                } else if (slot.dolu) {
                    slotHtml = `<div class="slot taken">Tutuldu</div>`;
                } else {
                    slotHtml = `<div class="slot free" onclick="slotSec('${s.sahaKodu}','${s.ad}',${saat},'${saatStr}',${s.saatlikUcret})">
            ✅ Rezervasyon Yap
          </div>`;
                }
            }

            html += `<td class="td-slot">${slotHtml}</td>`;
        });

        html += '</tr>';
    }

    html += '</tbody></table>';
    container.innerHTML = html;
}

function slotSec(sahaKodu, sahaAdi, baslangicSaati, saatEtiket, ucret) {
    secilenSaha = { sahaKodu, sahaAdi };
    secilenSaat = baslangicSaati;
    saatlikUcret = ucret;

    // Özet doldur
    const tarihObj = new Date(secilenTarih + 'T12:00:00');
    const tarihYazi = `${tarihObj.getDate()} ${AYLAR_UZUN[tarihObj.getMonth()]} ${tarihObj.getFullYear()}`;

    document.getElementById('modalSummary').innerHTML = `
    <div class="sum-row"><span>Saha</span><span>${sahaAdi}</span></div>
    <div class="sum-row"><span>Tarih</span><span>${tarihYazi}</span></div>
    <div class="sum-row"><span>Saat</span><span>${saatEtiket}</span></div>
    <div class="sum-row total"><span>Ödenecek Tutar</span><span>${ucret.toLocaleString('tr-TR')} ₺</span></div>
  `;

    // Formu sıfırla
    document.getElementById('payForm').style.display = 'block';
    document.getElementById('paySuccess').style.display = 'none';
    document.getElementById('payErr').style.display = 'none';
    document.getElementById('payBtn').disabled = false;
    document.getElementById('payBtn').textContent = 'ÖDEME YAP';

    document.getElementById('modalOverlay').classList.add('open');
}

function modalKapat() {
    document.getElementById('modalOverlay').classList.remove('open');
}

async function odemeYap() {
    const adSoyad = document.getElementById('fAdSoyad').value.trim();
    const email = document.getElementById('fEmail').value.trim();
    const telefon = document.getElementById('fTelefon').value.trim();
    const kartNo = document.getElementById('fKartNo').value.trim();
    const skt = document.getElementById('fSkt').value.trim();
    const cvv = document.getElementById('fCvv').value.trim();
    const kartIsim = document.getElementById('fKartIsim').value.trim();
    const errEl = document.getElementById('payErr');

    errEl.style.display = 'none';

    // Validasyon
    if (!adSoyad || !email) return hata('Ad soyad ve e-posta zorunludur.');
    if (!kartNo || kartNo.replace(/\s/g, '').length < 16) return hata('Geçerli bir kart numarası girin.');
    if (!skt || !/^\d{2}\/\d{2}$/.test(skt)) return hata('Son kullanma tarihi AA/YY formatında olmalıdır.');
    if (!cvv || cvv.length < 3) return hata('CVV 3 haneli olmalıdır.');
    if (!kartIsim) return hata('Kart sahibinin adını girin.');

    const btn = document.getElementById('payBtn');
    btn.disabled = true;
    btn.textContent = 'İŞLEM YAPILIYOR...';

    try {
        // 1. Rezervasyonu oluştur
        const rezRes = await fetch(`${API}/api/rezervasyon/olustur`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sahaKodu: secilenSaha.sahaKodu,
                adSoyad, email, telefon,
                tarih: secilenTarih,
                baslangicSaati: secilenSaat
            })
        });

        const rezData = await rezRes.json();
        if (!rezRes.ok) {
            btn.disabled = false;
            btn.textContent = 'ÖDEME YAP';
            return hata(rezData.mesaj || rezData.title || 'Rezervasyon oluşturulamadı.');
        }

        // 2. Ödeme onay callback (göstermelik - gerçek iyzico entegrasyonu için bu kısım değişecek)
        await fetch(`${API}/api/rezervasyon/odeme-callback`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ rezervasyonId: rezData.rezervasyonId, basarili: true, iyzicoOdemeId: 'DEMO-' + Date.now() })
        });

        // Başarı ekranı
        const tarihObj = new Date(secilenTarih + 'T12:00:00');
        document.getElementById('successDesc').innerHTML = `
      <b>${adSoyad}</b> adına rezervasyon tamamlandı.<br><br>
      📍 <b>${secilenSaha.sahaAdi}</b><br>
      📅 ${tarihObj.getDate()} ${AYLAR_UZUN[tarihObj.getMonth()]} ${tarihObj.getFullYear()}<br>
      ⏰ ${String(secilenSaat).padStart(2, '0')}:00 – ${String(secilenSaat + 1).padStart(2, '0')}:00<br>
      💳 ${saatlikUcret.toLocaleString('tr-TR')} ₺ ödendi<br><br>
      <span style="font-size:12px;">Onay e-postası <b>${email}</b> adresinize gönderildi.</span>
    `;
        document.getElementById('payForm').style.display = 'none';
        document.getElementById('paySuccess').style.display = 'block';

        // 3 saniye sonra kapat ve tabloyu yenile
        setTimeout(() => {
            modalKapat();
            tabloYukle();
        }, 4000);

    } catch (e) {
        btn.disabled = false;
        btn.textContent = 'ÖDEME YAP';
        hata('Sunucuya bağlanılamadı.');
    }
}

function hata(msg) {
    const el = document.getElementById('payErr');
    el.textContent = msg;
    el.style.display = 'block';
}

// Kart numarası formatla: 1234 5678 9012 3456
function kartFmt(inp) {
    let v = inp.value.replace(/\D/g, '').substring(0, 16);
    inp.value = v.replace(/(.{4})/g, '$1 ').trim();
}

// SKT formatla: MM/YY
function sktFmt(inp) {
    let v = inp.value.replace(/\D/g, '').substring(0, 4);
    if (v.length >= 3) v = v.substring(0, 2) + '/' + v.substring(2);
    inp.value = v;
}

function fmt(d) { return d.toISOString().split('T')[0]; }