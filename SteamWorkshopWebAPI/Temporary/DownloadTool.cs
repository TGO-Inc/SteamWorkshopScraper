﻿using SteamKit2;
using SteamKit2.CDN;
using SteamWorkshop.WebAPI.Internal;

namespace SteamWorkshop.WebAPI
{
    public class DownloadTool
    {
        private readonly Steam3Session Steam3;
        private readonly CDNClientPool CDNClientPool;
        private readonly Server CDNConnection;
        public DownloadTool(string username, string password, uint appid)
        {
            this.Steam3 = new Steam3Session(new SteamUser.LogOnDetails()
            {
                Username = username,
                Password = password,
                ShouldRememberPassword = true,
                LoginID = 0x534B32
            });
            this.CDNClientPool = new CDNClientPool(this.Steam3, appid);
            this.CDNConnection = this.CDNClientPool.GetConnection(new());
            this.Steam3.RequestDepotKey(appid, appid);
        }
        public DepotManifest DownloadManifest(uint depotid, uint appid, ulong manifestid)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var ManifestRequestCode = this.Steam3.GetDepotManifestRequestCodeAsync(
                                depotid,
                                appid,
                                manifestid,
                            "public").GetAwaiter().GetResult();

                    return this.CDNClientPool.CDNClient.DownloadManifestAsync(
                                        depotid,
                                        manifestid,
                                        ManifestRequestCode,
                                        this.CDNConnection,
                                        this.Steam3.DepotKeys[depotid],
                                        this.CDNClientPool.ProxyServer).GetAwaiter().GetResult();
                }
                catch
                {
                    Task.Delay(10).Wait();
                    continue;
                }
            }
            return null;
        }
        public byte[] DownloadFile(uint depotid, DepotManifest.ChunkData data)
        {
            var chunkData = this.CDNClientPool.CDNClient.DownloadDepotChunkAsync(
                        depotid,
                        data,
                        this.CDNConnection,
                        this.Steam3.DepotKeys[depotid],
                        this.CDNClientPool.ProxyServer).GetAwaiter().GetResult();

            return chunkData.Data;
        }
    }
}
