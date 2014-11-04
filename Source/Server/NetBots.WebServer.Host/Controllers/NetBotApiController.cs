﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Ajax.Utilities;
using NetBots.GameEngine;
using NetBots.Web;
using NetBots.WebServer.Data.MsSql;
using NetBots.WebServer.Host.Controllers;
using NetBots.WebServer.Host.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace NetBotsHostProject.Controllers
{
    [EnableCors("*", "*", "*")]
    public class NetBotApiController : ApiController
    {
        readonly ApplicationDbContext _db = new ApplicationDbContext();

        [Route("api/botlist")]
        public async Task<IHttpActionResult> GetBotList()
        {
            //todo: Get only visible ones once the private bots branch is merged in.
            var visibleBots = await _db.PlayerBots.ToListAsync();
            var returnObjects = visibleBots.Select(x => new BotIdApiModel() { Name = x.Name, Id = x.Id });
            return Ok(returnObjects);
        }

        [Route("api/startgame")]
        [HttpPost]
        public async Task<IHttpActionResult> StartGame(StartGameApiModel apiModel)
        {
            var battleController = new BattleController();
            var gamestate = battleController.GetNewGameState();
            var opponent = await _db.PlayerBots.FirstOrDefaultAsync(x => x.Id == apiModel.OpponentId);
            var opponentUrl = opponent.URL;
            var side1Url = apiModel.Side == "p1" ? null : opponentUrl;
            var side2Url = apiModel.Side == "p2" ? null : opponentUrl;
            var players = battleController.GetPlayers(20, side1Url, side2Url);
            var game = new Game(gamestate, players);
            HttpContext.Current.Cache.Add(gamestate.GameId, game, null, Cache.NoAbsoluteExpiration, new TimeSpan(0, 0, 10, 0), CacheItemPriority.High, null);
            return Ok(gamestate);
        }

        [Route("api/updategame")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateGameState(UpdateGameApiModel apiModel)
        {
            var game = HttpContext.Current.Cache[apiModel.GameState.GameId] as Game;
            if (game == null)
            {
                return BadRequest("Could not find that GameState in cache.");
            }
            var opponent = game.Players.First(x => x.Uri != null);
            var opponentMoves = await BattleController.GetPlayerMovesAsync(opponent, apiModel.GameState);
            game.UpdateGameState(new[] { apiModel.ClientMoves, opponentMoves });
            return Ok(game.GameState);
        }

        
    }

    public class BotIdApiModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class StartGameApiModel
    {
        [JsonProperty("opponentId")]
        public int OpponentId { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }
    }

    public class UpdateGameApiModel
    {
        [JsonProperty("gameState")]
        public GameState GameState { get; set; }

        [JsonProperty("clientMoves")]
        public PlayerMoves ClientMoves { get; set; }
    }
}
