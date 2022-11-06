﻿using HtmlAgilityPack;
using SpotifyAPI.Web;
using BbcPlaylistToSpotify;
using BbcPlaylistToSpotify.Extensions;
using System.Web;
using System;
using System.Diagnostics.Contracts;

try
{
    var appConfig = new AppConfig();
    var spotify = new SpotifyClient(appConfig.SpotifyApiToken ?? "");

    Console.WriteLine("Starting...");

    var playlist = await CreatePlaylist(appConfig, spotify);

    await AddTracks(appConfig, spotify, playlist);
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
    Console.WriteLine($"Hit <Enter> to finish");
    Console.ReadLine();
}

static async Task AddTracks(AppConfig appConfig, SpotifyClient spotify, FullPlaylist playlist)
{
    using (var httpClient = new HttpClient())
    {
        foreach (var bbcPlaylistUrl in appConfig.BbcPlaylistUrls)
        {
            Console.WriteLine($"Downloading playlist: {bbcPlaylistUrl}");
            var html = await httpClient.GetStringAsync(bbcPlaylistUrl);

            await AddBbcPlaylist(spotify, playlist, html);
        }

        Console.WriteLine("Done");
    }
}

static async Task AddBbcPlaylist(SpotifyClient spotify, FullPlaylist playlist, string bbcPlaylistHtml)
{
    if (playlist?.Id == null)
    {
        throw new ArgumentNullException("playlist.Id");
    }

    var doc = new HtmlDocument();
    doc.LoadHtml(bbcPlaylistHtml);

    var tracks = doc.DocumentNode
        .SelectNodes("//div[@class='text--prose']/p")
        .Where(x => x.LastChild.Name == "#text" || x.LastChild.Name == "strong")
        .Select(x => HttpUtility.HtmlDecode(x.InnerText));

    foreach (var track in tracks)
    {
        Console.WriteLine($"Adding track: {track}");

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, track);
        var searchResponse = await spotify.Search.Item(searchRequest);

        if (searchResponse.Tracks.Items != null && searchResponse.Tracks.Items.Any())
        {
            var trackUris = new PlaylistAddItemsRequest(new List<string>() { searchResponse.Tracks.Items.First().Uri });
            var addResponse = await spotify.Playlists.AddItems(playlist.Id, trackUris);
        }
    }
}

static string NewPlaylistName()
{
    var now = DateTime.UtcNow;
    var newPlaylistName = $"BbcPlaylist-{now.ToShortMonthName()}-{now.Year}";
    return newPlaylistName;
}

static async Task<FullPlaylist> CreatePlaylist(AppConfig appConfig, SpotifyClient spotify)
{
    var newPlaylistName = NewPlaylistName();
    var playlistCresteRequest = new PlaylistCreateRequest(newPlaylistName);
    var playlist = await spotify.Playlists.Create(appConfig.SpotifyUsername, playlistCresteRequest);

    Console.WriteLine($"Created spotify playlist: {newPlaylistName}");
    return playlist;
}