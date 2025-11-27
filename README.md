# 🎵 SpotyWrap

A Blazor-based web application that connects to your Spotify account and automatically organizes your liked songs into monthly playlists.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![Blazor](https://img.shields.io/badge/Blazor-Server-blue.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## ✨ Features

- 🔐 **Secure Spotify Authentication** - OAuth 2.0 with PKCE flow for enhanced security
- 📅 **Monthly Playlist Generation** - Automatically organize your liked songs by the month you added them
- 🎯 **Current Month Playlist** - Generate a playlist for the current month's liked songs
- 📚 **All-Time Organization** - Create playlists for all your liked songs, organized by month
- 🏆 **Monthly Top Playlist** - Combine your top 50 tracks with liked songs from the current month, with popularity-based ranking
- 💾 **Cookie-based Session** - Maintains your authentication state across sessions
- 🎨 **Modern UI** - Clean and responsive design with Bootstrap
- 📊 **Duplicate Detection** - Prevents creating duplicate playlists

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Spotify account
- Spotify Developer credentials (Client ID)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/Szabbi97/SpotyWrap.git
   cd SpotyWrap
   ```

2. **Configure Spotify API**
   
   Create a `appsettings.Development.json` file in the `SpotyWrap` directory:
   ```json
   {
     "Spotify": {
       "ClientId": "your-spotify-client-id"
     }
   }
   ```

3. **Set up Spotify Developer App**
   - Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
   - Create a new app
   - Add `https://localhost:5001/` to Redirect URIs
   - Copy your Client ID to the configuration file

4. **Run the application**
   ```bash
   cd SpotyWrap
   dotnet run
   ```

5. **Open your browser**
   
   Navigate to `https://localhost:5001`

## 🐳 Docker Deployment

The application includes a Dockerfile for containerized deployment.

### Build the Docker image
```bash
docker build -t spotywrap .
```

### Run the container
```bash
docker run -p 8080:8080 -e Spotify__ClientId="your-client-id" spotywrap
```

## 🌐 Deployment Options

The project includes configurations for:

- **Railway** - See `railway.json`
- **Fly.io** - See `fly.toml`

### Environment Variables

- `Spotify__ClientId` - Your Spotify application Client ID

## 📖 How It Works

1. **Authentication**: Users authenticate with Spotify using OAuth 2.0 PKCE flow
2. **Token Storage**: Access tokens are securely stored in HTTP-only cookies
3. **Data Fetching**: The app retrieves your liked songs and top tracks from Spotify API
4. **Playlist Generation**: 
   - **This Month**: Songs are grouped by the month they were added
   - **All Months**: Creates playlists for all months with names in `YYYY.M` format (e.g., "2024.1" for January 2024)
   - **Monthly Top**: Combines your top 50 tracks (short-term) with liked songs from the current month
     - Liked songs receive a +30 popularity boost
     - Final playlist is sorted by adjusted popularity
     - Limited to top 30 tracks
   - All playlists are created as private on your Spotify account

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0
- **UI**: Blazor Server with Bootstrap 5
- **Authentication**: Spotify OAuth 2.0 with PKCE
- **API**: Spotify Web API
- **Deployment**: Docker-ready with Railway and Fly.io support

## 📁 Project Structure

```
SpotyWrap/
├── Components/
│   ├── Pages/          # Blazor pages (Home, Generator, AllLiked, User)
│   ├── Classes/        # Data models (Track, Album, Playlist, etc.)
│   └── Layout/         # Layout components
├── Services/
│   └── AuthStateService.cs  # Authentication management
├── Configuration/
│   └── SpotifySettings.cs   # Configuration classes
├── wwwroot/
│   ├── spotify-auth.js      # JavaScript interop for auth
│   └── infinite-scroll.js   # Infinite scroll functionality
└── Program.cs               # Application entry point
```

## 🎮 Usage

### Generate This Month Playlist
1. Navigate to the Generator page
2. Click "Generate" under "This Month"
3. A playlist named `YYYY.M` will be created with all songs liked in the current month

### Generate All Playlists
1. Navigate to the Generator page
2. Click "Generate" under "All Months"
3. Playlists will be created for each month you have liked songs

### Generate Monthly Top
1. Navigate to the Generator page
2. Click "Generate" under "Monthly Top"
3. A curated playlist combining:
   - Your top 50 tracks (based on recent listening)
   - Songs you liked this month (with popularity boost)
4. The top 30 tracks from this combined list will be added to a playlist named `YYYY.M - Top`

### View All Liked Songs
- Navigate to the "All Liked" page to browse all your liked songs with infinite scroll

## 🔒 Security Features

- PKCE (Proof Key for Code Exchange) flow for OAuth
- HTTP-only cookies for token storage
- No client secrets in frontend code
- Session-based state management

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- Powered by [Spotify Web API](https://developer.spotify.com/documentation/web-api/)
- Icons from [Bootstrap Icons](https://icons.getbootstrap.com/)

## 📧 Contact

Szabbi97 - [GitHub](https://github.com/Szabbi97)

Project Link: [https://github.com/Szabbi97/SpotyWrap](https://github.com/Szabbi97/SpotyWrap)

---

⭐ If you find this project useful, please consider giving it a star!