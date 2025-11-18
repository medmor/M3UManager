window.VideoPlayerHelper = {
    hls: null,
    
    initPlayer: async function(videoId, streamUrl) {
        console.log('InitPlayer called with:', videoId, streamUrl);
        const video = document.getElementById(videoId);
        if (!video) {
            console.error('Video element not found:', videoId);
            return false;
        }

        console.log('Video element found');
        console.log('Stream URL:', streamUrl);
        console.log('Hls available:', typeof Hls !== 'undefined');
        console.log('Hls.isSupported:', typeof Hls !== 'undefined' ? Hls.isSupported() : 'N/A');

        // Destroy existing HLS instance if any
        if (this.hls) {
            console.log('Destroying existing HLS instance');
            this.hls.destroy();
            this.hls = null;
        }

        // Check if it's an HLS stream
        const isHlsStream = streamUrl.includes('.m3u8') || streamUrl.includes('/live/') || streamUrl.toLowerCase().includes('m3u');
        console.log('Is HLS stream:', isHlsStream);

        try {
            // Try to fetch the stream to check if it's accessible
            console.log('Testing stream accessibility...');
            const response = await fetch(streamUrl, { 
                mode: 'no-cors',
                method: 'HEAD'
            }).catch(err => {
                console.log('HEAD request failed, trying direct play:', err);
                return null;
            });
            
            if (response) {
                console.log('Stream is accessible');
            }
        } catch (e) {
            console.log('Stream test error:', e);
        }

        if (isHlsStream && typeof Hls !== 'undefined' && Hls.isSupported()) {
            console.log('Using HLS.js');
            try {
                this.hls = new Hls({
                    debug: true,
                    enableWorker: true,
                    lowLatencyMode: false,
                    backBufferLength: 90,
                    maxBufferLength: 30,
                    maxMaxBufferLength: 60,
                    manifestLoadingTimeOut: 20000,
                    manifestLoadingMaxRetry: 6,
                    levelLoadingTimeOut: 20000,
                    levelLoadingMaxRetry: 6,
                    fragLoadingTimeOut: 20000,
                    fragLoadingMaxRetry: 6,
                    xhrSetup: function(xhr, url) {
                        // Don't set withCredentials for CORS
                        console.log('XHR setup for URL:', url);
                    }
                });
                
                this.hls.on(Hls.Events.MEDIA_ATTACHED, function() {
                    console.log('HLS: Media attached');
                });

                this.hls.on(Hls.Events.MANIFEST_LOADING, function() {
                    console.log('HLS: Manifest loading');
                });
                
                this.hls.on(Hls.Events.MANIFEST_PARSED, function(event, data) {
                    console.log('HLS: Manifest parsed, levels:', data.levels.length);
                    video.play().then(() => {
                        console.log('Video playing successfully');
                    }).catch(e => {
                        console.error('Autoplay prevented:', e);
                    });
                });

                this.hls.on(Hls.Events.LEVEL_LOADED, function(event, data) {
                    console.log('HLS: Level loaded');
                });

                this.hls.on(Hls.Events.FRAG_LOADED, function(event, data) {
                    console.log('HLS: Fragment loaded');
                });
                
                this.hls.on(Hls.Events.ERROR, function(event, data) {
                    console.error('HLS Error:', data.type, data.details, data);
                    
                    if (data.fatal) {
                        console.error('Fatal error detected');
                        switch(data.type) {
                            case Hls.ErrorTypes.NETWORK_ERROR:
                                console.log('Network error, trying to recover...');
                                setTimeout(() => {
                                    this.hls.startLoad();
                                }, 1000);
                                break;
                            case Hls.ErrorTypes.MEDIA_ERROR:
                                console.log('Media error, trying to recover...');
                                this.hls.recoverMediaError();
                                break;
                            default:
                                console.error('Fatal error, cannot recover');
                                // Try fallback to direct video src
                                console.log('Trying fallback to direct video source');
                                this.hls.destroy();
                                this.hls = null;
                                video.src = streamUrl;
                                video.load();
                                video.play().catch(e => console.error('Fallback play error:', e));
                                break;
                        }
                    }
                }.bind(this));
                
                console.log('Loading source:', streamUrl);
                this.hls.loadSource(streamUrl);
                console.log('Attaching media');
                this.hls.attachMedia(video);
                
                return true;
            } catch (error) {
                console.error('Error initializing HLS:', error);
                return false;
            }
        } 
        else if (isHlsStream && video.canPlayType('application/vnd.apple.mpegurl')) {
            // Native HLS support (Safari)
            console.log('Using native HLS support');
            video.src = streamUrl;
            video.load();
            video.addEventListener('loadedmetadata', function() {
                console.log('Metadata loaded');
                video.play().catch(e => {
                    console.error('Autoplay prevented:', e);
                });
            });
            video.addEventListener('error', function(e) {
                console.error('Video error:', e, video.error);
            });
            return true;
        }
        else {
            // For non-HLS streams or fallback - set crossorigin attribute
            console.log('Using direct video source');
            video.crossOrigin = 'anonymous';
            video.src = streamUrl;
            video.load();
            video.addEventListener('loadedmetadata', function() {
                console.log('Metadata loaded');
            });
            video.addEventListener('error', function(e) {
                console.error('Video error:', e, video.error);
                if (video.error) {
                    console.error('Error code:', video.error.code);
                    console.error('Error message:', video.error.message);
                }
            });
            video.play().catch(e => {
                console.error('Play error:', e);
            });
            return true;
        }
    },
    
    play: function(videoId) {
        console.log('Play called');
        const video = document.getElementById(videoId);
        if (video) {
            video.play().catch(e => console.error('Play error:', e));
        }
    },
    
    pause: function(videoId) {
        console.log('Pause called');
        const video = document.getElementById(videoId);
        if (video) {
            video.pause();
        }
    },
    
    stop: function(videoId) {
        console.log('Stop called');
        const video = document.getElementById(videoId);
        if (video) {
            video.pause();
            video.currentTime = 0;
            video.src = '';
        }
        if (this.hls) {
            this.hls.destroy();
            this.hls = null;
        }
    },
    
    toggleFullscreen: function(videoId) {
        console.log('Toggle fullscreen called');
        const video = document.getElementById(videoId);
        if (video) {
            if (video.requestFullscreen) {
                video.requestFullscreen();
            } else if (video.webkitRequestFullscreen) {
                video.webkitRequestFullscreen();
            } else if (video.msRequestFullscreen) {
                video.msRequestFullscreen();
            }
        }
    },
    
    destroy: function() {
        console.log('Destroy called');
        if (this.hls) {
            this.hls.destroy();
            this.hls = null;
        }
    }
};

// Log when script loads
console.log('VideoPlayerHelper loaded');
console.log('Hls available:', typeof Hls !== 'undefined');
