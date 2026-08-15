// =====================================================
// API AYARLARI
// =====================================================

// Backend'in ana adresi
const API_BASE_URL = "https://localhost:7266";


// =====================================================
// GET İSTEKLERİ
// =====================================================

async function apiGet(endpoint) {

    try {

        const response = await fetch(
            `${API_BASE_URL}${endpoint}`,
            {
                method: "GET",

                headers: {
                    "Accept": "application/json"
                }
            }
        );


        // API başarısız cevap verdiyse
        if (!response.ok) {

            let mesaj =
                `API hatası: ${response.status}`;

            try {

                const hata =
                    await response.json();

                mesaj =
                    hata?.message ||
                    hata?.mesaj ||
                    hata?.error ||
                    mesaj;

            }
            catch {
                // JSON hata mesajı gelmediyse
                // HTTP durum kodu gösterilir.
            }


            throw new Error(mesaj);
        }


        // Başarılı JSON cevabı
        return await response.json();

    }
    catch (error) {

        console.error(
            "GET isteği başarısız:",
            error
        );

        throw error;
    }
}


// =====================================================
// POST İSTEKLERİ
// =====================================================

async function apiPost(endpoint, data) {

    try {

        const response = await fetch(
            `${API_BASE_URL}${endpoint}`,
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },

                body: JSON.stringify(data)
            }
        );


        // Backend bazen JSON body döndürmeyebilir.
        const result =
            await response
                .json()
                .catch(() => null);


        // API başarısız cevap verdiyse
        if (!response.ok) {

            throw new Error(
                result?.message ||
                result?.mesaj ||
                result?.error ||
                `API hatası: ${response.status}`
            );
        }


        // Başarılı API cevabı
        return result;

    }
    catch (error) {

        console.error(
            "POST isteği başarısız:",
            error
        );

        throw error;
    }
}