mergeInto(LibraryManager.library, {
  WA_Init: function(maxSeconds) {
    if (!window.__wa) {
      window.__wa = {
        maxSeconds: maxSeconds || 60,
        stream: null,
        recorder: null,
        chunks: [],
        blob: null,
        url: null,
        audio: null,
        recording: false,
        playing: false
      };
    } else {
      window.__wa.maxSeconds = maxSeconds || window.__wa.maxSeconds;
    }
  },

  WA_StartRecord: function() {
    const wa = window.__wa;
    if (!wa) return;
    if (wa.recording) return;

    wa.chunks = [];
    wa.blob = null;
    wa.playing = false;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      wa.stream = stream;
      wa.recorder = new MediaRecorder(stream);
      wa.recorder.ondataavailable = e => { if (e.data && e.data.size) wa.chunks.push(e.data); };
      wa.recorder.onstop = () => {
        wa.blob = new Blob(wa.chunks, { type: wa.recorder.mimeType || 'audio/webm' });
        if (wa.url) URL.revokeObjectURL(wa.url);
        wa.url = URL.createObjectURL(wa.blob);

        wa.audio = new Audio(wa.url);
        wa.audio.onended = () => { wa.playing = false; };
      };

      wa.recorder.start();
      wa.recording = true;

      // auto-stop (maxSeconds)
      setTimeout(() => {
        if (wa.recording) {
          wa.recorder.stop();
          wa.recording = false;
          if (wa.stream) wa.stream.getTracks().forEach(t => t.stop());
          wa.stream = null;
        }
      }, (wa.maxSeconds || 60) * 1000);

    }).catch(err => {
      console.error("Mic permission error:", err);
    });
  },

  WA_StopRecord: function() {
    const wa = window.__wa;
    if (!wa || !wa.recording) return;
    try {
      wa.recorder.stop();
    } catch(e) {}
    wa.recording = false;
    if (wa.stream) wa.stream.getTracks().forEach(t => t.stop());
    wa.stream = null;
  },

  WA_PlayPause: function() {
    const wa = window.__wa;
    if (!wa || !wa.audio) return;

    if (wa.audio.paused) {
      wa.audio.play();
      wa.playing = true;
    } else {
      wa.audio.pause();
      wa.playing = false;
    }
  },

  WA_Clear: function() {
    const wa = window.__wa;
    if (!wa) return;
    if (wa.audio) wa.audio.pause();
    wa.playing = false;
    wa.recording = false;
    wa.chunks = [];
    wa.blob = null;
    if (wa.url) URL.revokeObjectURL(wa.url);
    wa.url = null;
    wa.audio = null;
  },

  WA_HasAudio: function() {
    const wa = window.__wa;
    return (wa && wa.blob) ? 1 : 0;
  },

  WA_IsRecording: function() {
    const wa = window.__wa;
    return (wa && wa.recording) ? 1 : 0;
  },

  WA_IsPlaying: function() {
    const wa = window.__wa;
    return (wa && wa.playing) ? 1 : 0;
  },

  WA_GetDuration: function() {
    const wa = window.__wa;
    if (!wa || !wa.audio || !isFinite(wa.audio.duration)) return 0;
    return wa.audio.duration;
  },

  WA_GetTime: function() {
    const wa = window.__wa;
    if (!wa || !wa.audio) return 0;
    return wa.audio.currentTime || 0;
  },

  WA_Seek: function(t) {
    const wa = window.__wa;
    if (!wa || !wa.audio) return;
    wa.audio.currentTime = Math.max(0, t);
  },

  // Devuelve base64 (sin prefijo data:) de webm/opus para que lo mandes en tu POST final
  WA_GetAudioBase64: function() {
    const wa = window.__wa;
    if (!wa || !wa.blob) return allocate(intArrayFromString(""), 'i8', ALLOC_NORMAL);

    // Convert Blob -> base64 sync-like isn't possible; but we can store result in window.__wa_b64 with FileReader.
    // For simplicity, we expose last computed value; compute if missing.
    if (wa.__b64) {
      return allocate(intArrayFromString(wa.__b64), 'i8', ALLOC_NORMAL);
    }

    const reader = new FileReader();
    reader.onloadend = () => {
      const dataUrl = reader.result; // "data:audio/webm;base64,...."
      const b64 = (dataUrl.split(",")[1] || "");
      wa.__b64 = b64;
    };
    reader.readAsDataURL(wa.blob);

    // first call may return empty; call again a moment later before submit
    return allocate(intArrayFromString(wa.__b64 || ""), 'i8', ALLOC_NORMAL);
  }
});