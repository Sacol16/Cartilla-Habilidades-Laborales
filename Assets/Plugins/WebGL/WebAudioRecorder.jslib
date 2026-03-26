mergeInto(LibraryManager.library, {

  // ============================================================
  // RECORDER (WebAudioRecorderUI) — namespace: window.__wa
  // ============================================================
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
        playing: false,
        __b64: null
      };
    } else {
      window.__wa.maxSeconds = maxSeconds || window.__wa.maxSeconds;
    }
  },

  WA_StartRecord: function() {
    const wa = window.__wa;
    if (!wa || wa.recording) return;

    wa.chunks  = [];
    wa.blob    = null;
    wa.__b64   = null;
    wa.playing = false;

    navigator.mediaDevices.getUserMedia({ audio: true }).then(stream => {
      wa.stream = stream;

      const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
        ? 'audio/webm;codecs=opus'
        : 'audio/webm';

      wa.recorder = new MediaRecorder(stream, { mimeType });
      wa.recorder.ondataavailable = e => { if (e.data && e.data.size) wa.chunks.push(e.data); };
      wa.recorder.onstop = () => {
        wa.blob = new Blob(wa.chunks, { type: wa.recorder.mimeType || 'audio/webm' });
        if (wa.url) URL.revokeObjectURL(wa.url);
        wa.url = URL.createObjectURL(wa.blob);

        wa.audio = new Audio(wa.url);
        wa.audio.onended = () => { wa.playing = false; };

        // Pre-computar base64 inmediatamente
        const reader = new FileReader();
        reader.onloadend = () => { wa.__b64 = (reader.result.split(",")[1] || ""); };
        reader.readAsDataURL(wa.blob);
      };

      wa.recorder.start();
      wa.recording = true;

      setTimeout(() => {
        if (wa.recording) {
          wa.recorder.stop();
          wa.recording = false;
          if (wa.stream) wa.stream.getTracks().forEach(t => t.stop());
          wa.stream = null;
        }
      }, (wa.maxSeconds || 60) * 1000);

    }).catch(err => console.error("[WA] Mic error:", err));
  },

  WA_StopRecord: function() {
    const wa = window.__wa;
    if (!wa || !wa.recording) return;
    try { wa.recorder.stop(); } catch(e) {}
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
    if (wa.audio) { wa.audio.pause(); wa.audio.src = ""; }
    wa.playing   = false;
    wa.recording = false;
    wa.chunks    = [];
    wa.blob      = null;
    wa.__b64     = null;
    if (wa.url) URL.revokeObjectURL(wa.url);
    wa.url   = null;
    wa.audio = null;
  },

  WA_HasAudio:    function() { const wa = window.__wa; return (wa && wa.blob)      ? 1 : 0; },
  WA_IsRecording: function() { const wa = window.__wa; return (wa && wa.recording) ? 1 : 0; },
  WA_IsPlaying:   function() { const wa = window.__wa; return (wa && wa.playing)   ? 1 : 0; },

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

  WA_GetAudioBase64: function() {
    const wa = window.__wa;
    if (!wa || !wa.blob) return allocate(intArrayFromString(""), 'i8', ALLOC_NORMAL);
    return allocate(intArrayFromString(wa.__b64 || ""), 'i8', ALLOC_NORMAL);
  },

  // ============================================================
  // PRESENTER (Activity4Presenter) — namespace: window.__wa_p
  //
  // USA AudioContext + decodeAudioData en lugar de new Audio().
  // Esto evita la distorsión causada por:
  //   1. Conflicto con el AudioContext propio de Unity WebGL.
  //   2. Resampling incorrecto del elemento <audio> del DOM.
  //   3. Políticas de autoplay que suspenden el contexto.
  // ============================================================

  WAP_LoadBase64: function(base64Ptr) {
    const b64 = UTF8ToString(base64Ptr);
    if (!b64) { console.warn("[WAP] base64 vacío."); return; }

    // Limpiar estado anterior
    if (window.__wa_p) {
      const old = window.__wa_p;
      if (old.sourceNode) { try { old.sourceNode.stop(); } catch(e){} }
      if (old.ctx && old.ctx.state !== 'closed') old.ctx.close().catch(()=>{});
    }

    window.__wa_p = {
      ctx:        null,
      buffer:     null,
      sourceNode: null,
      startTime:  0,
      pausePos:   0,
      playing:    false,
      ready:      false
    };
    const wp = window.__wa_p;

    try {
      const binary = atob(b64);
      const bytes  = new Uint8Array(binary.length);
      for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);

      // AudioContext independiente del de Unity
      wp.ctx = new (window.AudioContext || window.webkitAudioContext)();

      wp.ctx.decodeAudioData(
        bytes.buffer,
        function(audioBuffer) {
          wp.buffer = audioBuffer;
          wp.ready  = true;
          console.log("[WAP] Decodificado OK — dur:", audioBuffer.duration.toFixed(2), "s | SR:", audioBuffer.sampleRate, "Hz");
        },
        function(err) {
          console.error("[WAP] decodeAudioData error:", err);
          wp.ready = false;
        }
      );
    } catch(e) {
      console.error("[WAP] LoadBase64 exception:", e);
    }
  },

  WAP_PlayPause: function() {
    const wp = window.__wa_p;
    if (!wp || !wp.ready || !wp.buffer) {
      console.warn("[WAP] PlayPause: no listo.");
      return;
    }

    const ctx = wp.ctx;
    if (ctx.state === 'suspended') ctx.resume();

    if (wp.playing) {
      // PAUSAR — guardar posición actual
      wp.pausePos = Math.min(ctx.currentTime - wp.startTime, wp.buffer.duration);
      if (wp.sourceNode) { try { wp.sourceNode.stop(); } catch(e){} wp.sourceNode = null; }
      wp.playing = false;
    } else {
      // PLAY desde pausePos
      const source  = ctx.createBufferSource();
      source.buffer = wp.buffer;
      source.connect(ctx.destination);

      const offset = Math.max(0, Math.min(wp.pausePos, wp.buffer.duration));
      wp.startTime = ctx.currentTime - offset;
      source.start(0, offset);

      source.onended = () => {
        if (wp.playing) {
          wp.playing    = false;
          wp.pausePos   = 0;
          wp.sourceNode = null;
        }
      };

      wp.sourceNode = source;
      wp.playing    = true;
    }
  },

  WAP_IsReady:   function() { const wp = window.__wa_p; return (wp && wp.ready)   ? 1 : 0; },
  WAP_IsPlaying: function() { const wp = window.__wa_p; return (wp && wp.playing) ? 1 : 0; },

  WAP_GetDuration: function() {
    const wp = window.__wa_p;
    if (!wp || !wp.buffer) return 0;
    return wp.buffer.duration;
  },

  WAP_GetTime: function() {
    const wp = window.__wa_p;
    if (!wp || !wp.buffer) return 0;
    if (wp.playing) return Math.min(wp.ctx.currentTime - wp.startTime, wp.buffer.duration);
    return wp.pausePos;
  },

  WAP_Seek: function(t) {
    const wp = window.__wa_p;
    if (!wp || !wp.ready || !wp.buffer) return;

    const wasPlaying = wp.playing;
    if (wp.sourceNode) { try { wp.sourceNode.stop(); } catch(e){} wp.sourceNode = null; }
    wp.playing  = false;
    wp.pausePos = Math.max(0, Math.min(t, wp.buffer.duration));

    if (wasPlaying) {
      const ctx    = wp.ctx;
      if (ctx.state === 'suspended') ctx.resume();
      const source = ctx.createBufferSource();
      source.buffer  = wp.buffer;
      source.connect(ctx.destination);
      wp.startTime   = ctx.currentTime - wp.pausePos;
      source.start(0, wp.pausePos);
      source.onended = () => {
        if (wp.playing) { wp.playing = false; wp.pausePos = 0; wp.sourceNode = null; }
      };
      wp.sourceNode = source;
      wp.playing    = true;
    }
  },

  WAP_Stop: function() {
    const wp = window.__wa_p;
    if (!wp) return;
    if (wp.sourceNode) { try { wp.sourceNode.stop(); } catch(e){} wp.sourceNode = null; }
    wp.playing  = false;
    wp.pausePos = 0;
  }
});
