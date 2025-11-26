window.checkForSpotifyCallback = function () {
    // Check if we're returning from Spotify OAuth
    const hash = window.location.hash;
    const search = window.location.search;
    
    // Check for authorization code (PKCE flow)
    if (search && search.includes('code=')) {
      const params = new URLSearchParams(search);
        const code = params.get('code');
        
        if (code) {
     // Store the code temporarily
      sessionStorage.setItem('spotify_auth_code', code);
   
   // Clear the URL
   window.history.replaceState(null, null, window.location.pathname);
   
            return code;
        }
    }
    
    // Check for access token (implicit flow - fallback)
    if (hash && hash.includes('access_token=')) {
        const params = new URLSearchParams(hash.substring(1));
        const accessToken = params.get('access_token');
    const expiresIn = params.get('expires_in');
  
        if (accessToken) {
          const expirationDate = new Date();
            expirationDate.setSeconds(expirationDate.getSeconds() + parseInt(expiresIn || 3600));
  
            document.cookie = `spotify_access_token=${accessToken}; expires=${expirationDate.toUTCString()}; path=/; SameSite=Lax`;
      
  window.history.replaceState(null, null, window.location.pathname);
            window.location.reload();
    }
    }
};

window.getSpotifyAuthCode = function () {
    const code = sessionStorage.getItem('spotify_auth_code');
    if (code) {
        sessionStorage.removeItem('spotify_auth_code');
        return code;
    }
    return null;
};

window.getSpotifyAccessToken = function () {
    const cookies = document.cookie.split(';');
  for (let cookie of cookies) {
        const [name, value] = cookie.trim().split('=');
 if (name === 'spotify_access_token') {
         return value;
        }
    }
    return null;
};

window.clearSpotifyAccessToken = function () {
    document.cookie = 'spotify_access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
};

// PKCE helper functions
window.generateCodeVerifier = function () {
    const array = new Uint8Array(32);
    window.crypto.getRandomValues(array);
    return base64UrlEncode(array);
};

window.generateCodeChallenge = async function (codeVerifier) {
    const encoder = new TextEncoder();
    const data = encoder.encode(codeVerifier);
    const digest = await window.crypto.subtle.digest('SHA-256', data);
    return base64UrlEncode(new Uint8Array(digest));
};

function base64UrlEncode(array) {
    return btoa(String.fromCharCode.apply(null, array))
  .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');
}
