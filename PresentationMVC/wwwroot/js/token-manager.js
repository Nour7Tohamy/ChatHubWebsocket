/**
 * TokenManager — كل تاب عنده session مستقلة
 * sessionStorage  → للـ SignalR فقط
 * HttpOnly Cookie → بيتعمل من السيرفر، للـ MVC navigation
 */
const TokenManager = {

    setToken(data) {
        // sessionStorage خاص بالتاب الحالي بس
        sessionStorage.setItem('token', data.token || '');
        sessionStorage.setItem('userId', data.userId || '');
        sessionStorage.setItem('displayName', data.displayName || '');
    },

    getToken() {
        return sessionStorage.getItem('token') || '';
    },

    getUser() {
        return {
            userId: sessionStorage.getItem('userId') || '',
            displayName: sessionStorage.getItem('displayName') || ''
        };
    },

    isTokenValid() {
        const token = this.getToken();
        if (!token) return false;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.exp * 1000 > Date.now();
        } catch {
            return false;
        }
    },

    clear() {
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('userId');
        sessionStorage.removeItem('displayName');
    }
};