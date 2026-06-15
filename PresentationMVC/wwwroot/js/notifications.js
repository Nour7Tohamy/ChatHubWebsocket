/**
 * notifications.js — ChatHub
 * ─────────────────────────────────────────────────────────────────
 *  • Real-time badge counter (SignalR)
 *  • Toast popup with sound
 *  • Dropdown with latest notifications
 *  • Auto-fetch on page load
 * ─────────────────────────────────────────────────────────────────
 */

(function () {
    'use strict';

    const SOUND_ENABLED = true;
    const TOAST_DURATION_MS = 4500;
    const DROPDOWN_COUNT = 8;

    const badge = document.getElementById('js-notif-badge');
    const bell = document.getElementById('js-notif-btn');

    let unreadCount = 0;
    let dropdownOpen = false;
    let dropdownEl = null;

    /* ══════════════════════════════════════
       BADGE
    ══════════════════════════════════════ */
    function setBadge(n) {
        unreadCount = Math.max(0, n);
        if (!badge) return;
        if (unreadCount === 0) {
            badge.classList.remove('visible');
            badge.textContent = '';
        } else {
            badge.textContent = unreadCount > 99 ? '99+' : unreadCount;
            badge.classList.add('visible');
        }
    }

    function incrementBadge() { setBadge(unreadCount + 1); }

    /* ══════════════════════════════════════
       SOUND  — مكشوف للخارج عشان Room يستخدمه
    ══════════════════════════════════════ */
    function playSound() {
        if (!SOUND_ENABLED) return;
        try {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (!AudioCtx) return;
            const ctx = new AudioCtx();
            const gain = ctx.createGain();
            gain.connect(ctx.destination);
            gain.gain.setValueAtTime(0, ctx.currentTime);
            gain.gain.linearRampToValueAtTime(0.35, ctx.currentTime + 0.01);
            gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.55);
            [[523, 0], [659, 0.18]].forEach(([freq, delay]) => {
                const osc = ctx.createOscillator();
                osc.type = 'sine';
                osc.frequency.value = freq;
                osc.connect(gain);
                osc.start(ctx.currentTime + delay);
                osc.stop(ctx.currentTime + delay + 0.35);
            });
            setTimeout(() => ctx.close(), 800);
        } catch (_) { }
    }

    // ✅ اعمل playSound متاح globally عشان Room.cshtml يقدر يناديه
    window.ChatHubPlaySound = playSound;

    /* ══════════════════════════════════════
       TOAST STYLES
    ══════════════════════════════════════ */
    (function injectToastStyles() {
        if (document.getElementById('notif-toast-styles')) return;
        const s = document.createElement('style');
        s.id = 'notif-toast-styles';
        s.textContent = `
            #notif-toast-container {
                position:fixed; bottom:24px; right:24px;
                display:flex; flex-direction:column-reverse; gap:10px;
                z-index:9999; pointer-events:none;
            }
            .n-toast {
                pointer-events:auto; display:flex; align-items:flex-start; gap:12px;
                background:var(--bg-2); border:1px solid var(--border-3);
                border-radius:14px; padding:14px 16px;
                min-width:290px; max-width:360px;
                box-shadow:0 8px 32px rgba(0,0,0,0.25);
                opacity:0; transform:translateY(12px);
                transition:opacity .28s ease,transform .28s ease;
                cursor:pointer;
            }
            .n-toast.n-toast--show { opacity:1; transform:translateY(0); }
            .n-toast.n-toast--hide { opacity:0; transform:translateY(12px); }
            .n-toast__icon {
                width:36px; height:36px; border-radius:10px;
                background:var(--accent-dim);
                display:flex; align-items:center; justify-content:center; flex-shrink:0;
            }
            .n-toast__icon svg { width:18px; height:18px; stroke:var(--accent-2); fill:none; stroke-width:2; }
            .n-toast__body { flex:1; }
            .n-toast__title { font-size:13.5px; font-weight:600; color:var(--text-1); line-height:1.3; }
            .n-toast__sub   { font-size:12px; color:var(--text-2); margin-top:3px; }
            .n-toast__close {
                background:none; border:none; cursor:pointer;
                color:var(--text-3); padding:2px; line-height:1; transition:color .15s;
            }
            .n-toast__close:hover { color:var(--text-1); }
            @media (max-width:480px) {
                #notif-toast-container { right:12px; left:12px; bottom:16px; }
                .n-toast { max-width:100%; }
            }
        `;
        document.head.appendChild(s);
    })();

    function getOrCreateToastContainer() {
        let el = document.getElementById('notif-toast-container');
        if (!el) {
            el = document.createElement('div');
            el.id = 'notif-toast-container';
            document.body.appendChild(el);
        }
        return el;
    }

    function showToast({ title, preview, url }) {
        const container = getOrCreateToastContainer();
        const toast = document.createElement('div');
        toast.className = 'n-toast';
        toast.innerHTML = `
            <div class="n-toast__icon">
                <svg viewBox="0 0 24 24" stroke-linecap="round">
                    <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9"/>
                    <path d="M13.73 21a2 2 0 0 1-3.46 0"/>
                </svg>
            </div>
            <div class="n-toast__body">
                <div class="n-toast__title">${escHtml(title)}</div>
                ${preview ? `<div class="n-toast__sub">${escHtml(preview)}</div>` : ''}
            </div>
            <button class="n-toast__close" aria-label="Close">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/>
                    <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
            </button>`;

        if (url) {
            toast.addEventListener('click', e => {
                if (!e.target.closest('.n-toast__close'))
                    window.location.href = url;
            });
        }
        toast.querySelector('.n-toast__close')
            .addEventListener('click', () => dismissToast(toast));

        container.appendChild(toast);
        requestAnimationFrame(() =>
            requestAnimationFrame(() => toast.classList.add('n-toast--show'))
        );
        toast._timer = setTimeout(() => dismissToast(toast), TOAST_DURATION_MS);
    }

    // ✅ اعمل showToast متاح globally
    window.ChatHubShowToast = showToast;

    function dismissToast(toast) {
        clearTimeout(toast._timer);
        toast.classList.replace('n-toast--show', 'n-toast--hide');
        setTimeout(() => toast.remove(), 350);
    }

    /* ══════════════════════════════════════
       DROPDOWN STYLES
    ══════════════════════════════════════ */
    (function injectDropdownStyles() {
        if (document.getElementById('notif-dropdown-styles')) return;
        const s = document.createElement('style');
        s.id = 'notif-dropdown-styles';
        s.textContent = `
            #notif-dropdown {
                position:absolute; top:calc(100% + 10px); right:0;
                width:320px; background:var(--bg-2);
                border:1px solid var(--border-2); border-radius:16px;
                box-shadow:0 16px 48px rgba(0,0,0,0.25);
                z-index:9000; overflow:hidden;
                animation:ndropIn .2s ease;
            }
            @keyframes ndropIn {
                from { opacity:0; transform:translateY(-6px) scale(.98); }
                to   { opacity:1; transform:translateY(0) scale(1); }
            }
            .nd-header {
                display:flex; justify-content:space-between; align-items:center;
                padding:14px 16px 10px; border-bottom:1px solid var(--border);
            }
            .nd-header-title { font-size:13.5px; font-weight:600; color:var(--text-1); }
            .nd-mark-all {
                font-size:11.5px; color:var(--accent); background:none;
                border:none; cursor:pointer; padding:0; transition:color .15s;
            }
            .nd-mark-all:hover { color:var(--accent-2); }
            .nd-list { max-height:340px; overflow-y:auto; }
            .nd-list::-webkit-scrollbar { width:4px; }
            .nd-list::-webkit-scrollbar-thumb { background:var(--border-3); border-radius:2px; }
            .nd-item {
                display:flex; gap:12px; align-items:flex-start;
                padding:12px 16px; border-bottom:1px solid var(--border);
                text-decoration:none; transition:background .15s; cursor:pointer;
            }
            .nd-item:hover { background:var(--accent-dim); }
            .nd-item--unread { background:rgba(124,106,255,0.05); }
            .nd-dot {
                width:7px; height:7px; border-radius:50%;
                background:var(--accent); flex-shrink:0; margin-top:5px; transition:opacity .2s;
            }
            .nd-item--read .nd-dot { opacity:0; }
            .nd-text { flex:1; }
            .nd-title { font-size:13px; color:var(--text-1); line-height:1.35; }
            .nd-time  { font-size:11px; color:var(--text-3); margin-top:4px; }
            .nd-empty { text-align:center; padding:36px 16px; font-size:13px; color:var(--text-3); }
            .nd-footer { padding:10px 16px; border-top:1px solid var(--border); text-align:center; }
            .nd-footer a { font-size:12.5px; color:var(--accent); text-decoration:none; }
            .nd-footer a:hover { color:var(--accent-2); }
        `;
        document.head.appendChild(s);
    })();

    async function openDropdown() {
        if (dropdownOpen) { closeDropdown(); return; }
        dropdownOpen = true;

        const wrapper = bell.parentElement;
        wrapper.style.position = 'relative';

        dropdownEl = document.createElement('div');
        dropdownEl.id = 'notif-dropdown';
        dropdownEl.innerHTML = `
            <div class="nd-header">
                <span class="nd-header-title">Notifications</span>
                <button class="nd-mark-all" id="nd-mark-all-btn">Mark all as read</button>
            </div>
            <div class="nd-list" id="nd-list"><div class="nd-empty">Loading…</div></div>
            <div class="nd-footer"><a href="/Notifications">View all notifications</a></div>`;

        wrapper.appendChild(dropdownEl);
        dropdownEl.querySelector('#nd-mark-all-btn').addEventListener('click', markAllRead);
        await fetchAndRenderDropdown();
        setTimeout(() => document.addEventListener('click', outsideClickHandler), 0);
    }

    function closeDropdown() {
        dropdownOpen = false;
        dropdownEl?.remove();
        dropdownEl = null;
        document.removeEventListener('click', outsideClickHandler);
    }

    function outsideClickHandler(e) {
        if (dropdownEl && !dropdownEl.contains(e.target) && !bell.contains(e.target))
            closeDropdown();
    }

    async function fetchAndRenderDropdown() {
        const list = document.getElementById('nd-list');
        if (!list) return;
        try {
            const res = await fetch('/api/notifications/latest', { credentials: 'same-origin' });
            const data = await res.json();
            if (!data || data.length === 0) {
                list.innerHTML = '<div class="nd-empty">No notifications yet 🎉</div>';
                return;
            }
            list.innerHTML = data.slice(0, DROPDOWN_COUNT).map(n => `
                <a class="nd-item ${n.isRead ? 'nd-item--read' : 'nd-item--unread'}"
                   href="${escHtml(n.url || '/Notifications')}">
                    <span class="nd-dot"></span>
                    <div class="nd-text">
                        <div class="nd-title">${escHtml(n.title)}</div>
                        <div class="nd-time">${formatTime(n.createdAt)}</div>
                    </div>
                </a>`).join('');
        } catch (_) {
            list.innerHTML = '<div class="nd-empty">Failed to load notifications.</div>';
        }
    }

    async function markAllRead() {
        try {
            await fetch('/api/notifications/mark-all-read', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'RequestVerificationToken':
                        document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });
            setBadge(0);
            await fetchAndRenderDropdown();
        } catch (_) { }
    }

    /* ══════════════════════════════════════
       FETCH INITIAL COUNT
    ══════════════════════════════════════ */
    async function fetchUnreadCount() {
        try {
            const res = await fetch('/api/notifications/unread-count', { credentials: 'same-origin' });
            const count = await res.json();
            setBadge(count);
        } catch (_) { }
    }

    /* ══════════════════════════════════════
       SIGNALR HOOK
    ══════════════════════════════════════ */
    function hookSignalR(connection) {
        connection.on('ReceiveNotification', payload => {
            incrementBadge();
            playSound();
            showToast({
                title: payload?.senderName
                    ? `${payload.senderName} sent you a message`
                    : 'New notification',
                preview: payload?.preview || null,
                url: payload?.url || '/Notifications'
            });
        });
    }

    /* ══════════════════════════════════════
       BELL CLICK
    ══════════════════════════════════════ */
    if (bell) {
        bell.addEventListener('click', e => {
            e.preventDefault();
            openDropdown();
        });
    }

    /* ══════════════════════════════════════
       INIT
    ══════════════════════════════════════ */
    function waitAndInit(retries = 40, interval = 150) {
        if (window.ChatConfig?.connection) {
            hookSignalR(window.ChatConfig.connection);
        } else if (retries > 0) {
            setTimeout(() => waitAndInit(retries - 1, interval), interval);
        }
    }

    fetchUnreadCount();
    waitAndInit();

    /* ══════════════════════════════════════
       UTILS
    ══════════════════════════════════════ */
    function escHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function formatTime(dateStr) {
        if (!dateStr) return '';
        try {
            const d = new Date(dateStr);
            const diff = (Date.now() - d.getTime()) / 1000;
            if (diff < 60) return 'Just now';
            if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
            if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
            return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
        } catch { return ''; }
    }

})();