using System;
using System.Linq;
using AutoMapper;
// using Pulse.Entities;
// using Pulse.Models;

namespace Pulse.Configuration
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            // CreateMap<Player, PlayerModel>()
            //     .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar ?? src.CgeAvatarUrl))
            //     .ForMember(dest => dest.TotalGames, opt => opt.MapFrom(src => src.Matches.Count()))
            //     .ForMember(dest => dest.TotalTimeouts, opt => opt.MapFrom(src => src.Matches.Count(x => x.Score == -2)))
            //     .ForMember(dest => dest.TotalResigns, opt => opt.MapFrom(src => src.Matches.Count(x => x.Score == -1 && !x.IsWin)))
            //     .ForMember(dest => dest.TotalCulture, opt => opt.MapFrom(src => src.Matches.Aggregate(0, (sum, x) => sum + (x.Score > 0 ? x.Score : 0))))
            //     .ForMember(dest => dest.TotalWins, opt => opt.MapFrom(src => src.Matches.Count(x => x.IsWin)));

            // CreateMap<PlayerBadge, BadgeModel>()
            //     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Type.ToString()));

            // CreateMap<MatchPlayer, OpponentModel>()
            //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PlayerId))
            //     .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Player.Avatar ?? src.Player.CgeAvatarUrl))
            //     .ForMember(dest => dest.CgeUsername, opt => opt.MapFrom(src => src.Player.CgeUsername))
            //     .ForMember(dest => dest.Division, opt => opt.MapFrom(src => src.Player.Division))
            //     .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Player.Level))
            //     .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Player.Country))
            //     .ForMember(dest => dest.Cards, opt => opt.MapFrom(src => src.Cards.Select(x => x.Card.CgeName)))
            //     .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.Score == -2))
            //     .ForMember(dest => dest.IsResigned, opt => opt.MapFrom(src => src.Score == -1));

            // CreateMap<Match, MatchModel>()
            //     .ForMember(dest => dest.Opponents, opt => opt.MapFrom(src => src.MatchPlayers));

            // CreateMap<LeaderboardLog, LeaderboardLogModel>()
            //     .ForMember(dest => dest.CgeUsername, opt => opt.MapFrom(src => src.Player.CgeUsername))
            //     .ForMember(dest => dest.LeaderboardRating, opt => opt.MapFrom(src => src.ConservativeRating - src.TotalDecay));
        }
    }
}