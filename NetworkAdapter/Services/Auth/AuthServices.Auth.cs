using NetworkAdapter.Brokers;
using NetworkAdapter.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NetworkAdapter.Services
{
    public partial class AuthServices
    {
        public async Task<UserInfo> LoginUserAcync(string userId, string gamecentertoken)
        {
            try
            {
                var model = await httpClient.Post<object, UserInfo>
                    (loginUri,
                    new { userId = userId, loginToken = gamecentertoken });

                this.UserInfo = model;
                this.UserInfo.gameCenterToken = gamecentertoken;

                model.gameCenterToken = gamecentertoken;

                httpClient.SetUserInfoExternaly(model);

                return model;
            }
            catch (HttpRequestException e)
            {
                throw new ApiException($"http exception code {e.HResult}", e.Message);
            }
            catch (Exception e)
            {
                throw new ApiException("api call exception", e.Message);
            }
        }



    }
}
