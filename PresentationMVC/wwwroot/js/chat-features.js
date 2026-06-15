/**
 * chat-features.js
 * ─────────────────────────────────────────────────────────────────
 *  • Push-to-talk voice recording (hold = record, release = send)
 *  • Typing indicators (room & private)
 *  • Consistent SVG icons
 *
 *  Depends on window.ChatConfig being set by the page script:
 *  {
 *    connection,        // SignalR HubConnection (already started)
 *    currentUserId,
 *    currentDisplayName,
 *    mode,             // "room" | "private"
 *    roomId,           // string | null
 *    targetUserId      // string | null
 *  }
 * ─────────────────────────────────────────────────────────────────
 */

// ══════════════════════════════════════════════════════════════════
//  SVG ICONS
// ══════════════════════════════════════════════════════════════════
const ICONS = {
    mic: `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18"
               viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2"
               stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 1a3 3 0 0 0-3 3v8a3 3 0 0 0 6 0V4a3 3 0 0 0-3-3z"/>
            <path d="M19 10v2a7 7 0 0 1-14 0v-2"/>
            <line x1="12" y1="19" x2="12" y2="23"/>
            <line x1="8"  y1="23" x2="16" y2="23"/>
          </svg>`,

    micOff: `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                  viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" stroke-width="2"
                  stroke-linecap="round" stroke-linejoin="round">
               <line x1="1" y1="1" x2="23" y2="23"/>
               <path d="M9 9v3a3 3 0 0 0 5.12 2.12M15 9.34V4a3 3 0 0 0-5.94-.6"/>
               <path d="M17 16.95A7 7 0 0 1 5 12v-2m14 0v2a7 7 0 0 1-.11 1.23"/>
               <line x1="12" y1="19" x2="12" y2="23"/>
               <line x1="8"  y1="23" x2="16" y2="23"/>
             </svg>`,

    send: `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                viewBox="0 0 24 24" fill="none"
                stroke="currentColor" stroke-width="2"
                stroke-linecap="round" stroke-linejoin="round">
             <line x1="22" y1="2" x2="11" y2="13"/>
             <polygon points="22 2 15 22 11 13 2 9 22 2"/>
           </svg>`
};

// ══════════════════════════════════════════════════════════════════
//  WAIT FOR ChatConfig
//  بيستنى ChatConfig اللي فيه connection + mode (مش بس connection)
//  عشان نتجنب الـ ChatConfig الناقص من الـ Layout
// ══════════════════════════════════════════════════════════════════
function waitForConfig(cb, retries = 40, interval = 150) {
    if (window.ChatConfig?.connection && window.ChatConfig?.mode) {
        return cb(window.ChatConfig);
    }
    if (retries <= 0) {
        console.warn('[chat-features] ChatConfig never appeared — voice & typing disabled.');
        const recBtn = document.getElementById('recordBtn');
        if (recBtn) {
            recBtn.disabled = true;
            recBtn.title = 'Chat not connected';
        }
        return;
    }
    setTimeout(() => waitForConfig(cb, retries - 1, interval), interval);
}

// ══════════════════════════════════════════════════════════════════
//  MAIN INIT
// ══════════════════════════════════════════════════════════════════
waitForConfig(cfg => {
    const { connection, mode, roomId, targetUserId, currentDisplayName } = cfg;

    injectButtonIcons();
    initTyping(connection, mode, roomId, targetUserId, currentDisplayName);
    initVoice(connection, mode, roomId, targetUserId);
});

// ══════════════════════════════════════════════════════════════════
//  ICON INJECTION
// ══════════════════════════════════════════════════════════════════
function injectButtonIcons() {
    const sendBtn = document.getElementById('send-btn');
    if (sendBtn) {
        sendBtn.innerHTML = ICONS.send;
        sendBtn.title = 'Send';
        sendBtn.setAttribute('aria-label', 'Send message');
    }

    const recBtn = document.getElementById('recordBtn');
    if (recBtn) {
        recBtn.innerHTML = ICONS.mic;
        recBtn.title = 'Hold to record voice';
        recBtn.setAttribute('aria-label', 'Record voice message');
    }
}

// ══════════════════════════════════════════════════════════════════
//  TYPING INDICATOR
// ══════════════════════════════════════════════════════════════════
function initTyping(connection, mode, roomId, targetUserId, displayName) {
    const input = document.getElementById('messageInput');
    if (!input) return;

    let typingTimer = null;
    let isTyping = false;

    async function sendTyping(state) {
        try {
            if (mode === 'room' && roomId) {
                await connection.invoke(
                    state ? 'StartTypingInRoom' : 'StopTypingInRoom',
                    roomId
                );
            } else if (mode === 'private' && targetUserId) {
                await connection.invoke(
                    state ? 'StartTypingPrivate' : 'StopTypingPrivate',
                    targetUserId
                );
            }
        } catch { /* ignore */ }
    }

    input.addEventListener('input', () => {
        if (!isTyping) { isTyping = true; sendTyping(true); }
        clearTimeout(typingTimer);
        typingTimer = setTimeout(() => { isTyping = false; sendTyping(false); }, 2000);
    });

    input.addEventListener('blur', () => {
        if (isTyping) { isTyping = false; clearTimeout(typingTimer); sendTyping(false); }
    });
}

// ══════════════════════════════════════════════════════════════════
//  VOICE RECORDING
// ══════════════════════════════════════════════════════════════════
function initVoice(connection, mode, roomId, targetUserId) {
    const recBtn = document.getElementById('recordBtn');
    const badge = document.getElementById('recordingBadge');
    if (!recBtn) return;

    let mediaRecorder = null;
    let audioChunks = [];
    let recordingActive = false;
    let mediaStream = null;

    // ── best supported MIME ──────────────────────────────────────
    function getSupportedMime() {
        const types = [
            'audio/webm;codecs=opus',
            'audio/webm',
            'audio/ogg;codecs=opus',
            'audio/ogg',
            'audio/mp4',
        ];
        return types.find(t => MediaRecorder.isTypeSupported(t)) || '';
    }

    // ── start recording ──────────────────────────────────────────
    async function startRecording() {
        if (recordingActive) return;
        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        } catch (err) {
            alert('Microphone access denied.\n\n' + err.message);
            return;
        }

        audioChunks = [];
        const mime = getSupportedMime();
        const opts = mime ? { mimeType: mime } : {};

        try {
            mediaRecorder = new MediaRecorder(mediaStream, opts);
        } catch (err) {
            console.error('[chat-features] MediaRecorder init failed:', err);
            stopStream();
            alert('Could not start recorder: ' + err.message);
            return;
        }

        mediaRecorder.ondataavailable = e => {
            if (e.data && e.data.size > 0) audioChunks.push(e.data);
        };

        mediaRecorder.onstop = async () => {
            if (audioChunks.length === 0) return;
            const blob = new Blob(audioChunks, { type: mediaRecorder.mimeType || 'audio/webm' });
            audioChunks = [];
            await uploadAndSend(blob, mediaRecorder.mimeType || 'audio/webm');
        };

        mediaRecorder.start(200);
        recordingActive = true;

        recBtn.innerHTML = ICONS.micOff;
        recBtn.title = 'Release to send';
        recBtn.style.background = '#ef4444';
        recBtn.style.boxShadow = '0 0 0 4px rgba(239,68,68,.25)';
        if (badge) badge.classList.remove('d-none');
    }

    // ── stop recording ───────────────────────────────────────────
    function stopRecording() {
        if (!recordingActive) return;
        recordingActive = false;

        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
        }
        stopStream();

        recBtn.innerHTML = ICONS.mic;
        recBtn.title = 'Hold to record voice';
        recBtn.style.background = '';
        recBtn.style.boxShadow = '';
        if (badge) badge.classList.add('d-none');
    }

    function stopStream() {
        mediaStream?.getTracks().forEach(t => t.stop());
        mediaStream = null;
    }

    // ── upload → SignalR ─────────────────────────────────────────
    // ✅ FIX: بنستخدم Cookie authentication بدل JWT header
    // عشان الـ sessionStorage بيتبلوك من Tracking Prevention في بعض المتصفحات
    async function uploadAndSend(blob, mimeType) {
        const ext = mimeType.includes('ogg') ? '.ogg'
            : mimeType.includes('mp4') ? '.mp4'
                : '.webm';

        const fd = new FormData();
        fd.append('audio', blob, `voice${ext}`);

        let audioUrl;
        try {
            // ✅ لا نبعت Authorization header — الـ Cookie بيتبعت تلقائياً
            // والـ VoiceMessageController بيستخدم [Authorize] عادي (Cookie)
            const res = await fetch('/api/VoiceMessage/upload', {
                method: 'POST',
                credentials: 'same-origin', // ← مهم عشان الـ Cookie يتبعت
                body: fd
            });

            if (!res.ok) {
                let errMsg = `HTTP ${res.status}`;
                try {
                    const errJson = await res.json();
                    errMsg = errJson.error || errMsg;
                } catch { /* ignore */ }
                throw new Error(errMsg);
            }

            const json = await res.json();
            audioUrl = json.audioUrl;

        } catch (err) {
            console.error('[chat-features] Upload failed:', err);
            alert('Voice upload failed: ' + err.message);
            return;
        }

        // ── relay through SignalR ────────────────────────────────
        try {
            if (mode === 'room' && roomId) {
                await connection.invoke('SendVoiceMessage', roomId, audioUrl);
            } else if (mode === 'private' && targetUserId) {
                await connection.invoke('SendPrivateVoice', targetUserId, audioUrl);
            }
        } catch (err) {
            console.error('[chat-features] SignalR voice send failed:', err);
            alert('Could not deliver voice message: ' + err.message);
        }
    }

    // ── events: mouse + touch ────────────────────────────────────
    recBtn.addEventListener('mousedown', e => { e.preventDefault(); startRecording(); });
    recBtn.addEventListener('mouseup', () => stopRecording());
    recBtn.addEventListener('mouseleave', () => { if (recordingActive) stopRecording(); });

    recBtn.addEventListener('touchstart', e => { e.preventDefault(); startRecording(); }, { passive: false });
    recBtn.addEventListener('touchend', e => { e.preventDefault(); stopRecording(); }, { passive: false });
    recBtn.addEventListener('touchcancel', () => { if (recordingActive) stopRecording(); });
}