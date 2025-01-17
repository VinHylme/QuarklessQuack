﻿/*
 * Developer: Ramtin Jokar [ Ramtinak@live.com ] [ My Telegram Account: https://t.me/ramtinak ]
 * 
 * Github source: https://github.com/ramtinak/InstagramApiSharp
 * Nuget package: https://www.nuget.org/packages/InstagramApiSharp
 * 
 * IRANIAN DEVELOPERS
 */

using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using InstagramApiSharp.Enums;
using InstagramApiSharp.API.Versions;
using InstagramApiSharp.Helpers;
namespace InstagramApiSharp
{
    internal static class ExtensionHelper
    {
        public static string GenerateUserAgent(this AndroidDevice deviceInfo, InstaApiVersion apiVersion)
        {
            if (deviceInfo == null)
                return InstaApiConstants.USER_AGENT_DEFAULT;
            if (deviceInfo.AndroidVer == null)
                deviceInfo.AndroidVer = AndroidVersion.GetRandomAndriodVersion();

            return string.Format(InstaApiConstants.USER_AGENT, deviceInfo.Dpi, deviceInfo.Resolution, deviceInfo.HardwareManufacturer,
                deviceInfo.DeviceModelIdentifier, deviceInfo.FirmwareBrand, deviceInfo.HardwareModel,
                apiVersion.AppVersion, deviceInfo.AndroidVer.APILevel,
                deviceInfo.AndroidVer.VersionNumber, apiVersion.AppApiVersionCode, deviceInfo.AndroidBoardName);
        }
        public static string GenerateFacebookUserAgent()
        {
            var deviceInfo = AndroidDeviceGenerator.GetRandomAndroidDevice();
            //Mozilla/5.0 (Linux; Android 7.0; PRA-LA1 Build/HONORPRA-LA1; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/69.0.3497.100 Mobile Safari/537.36

            return string.Format(InstaApiConstants.FACEBOOK_USER_AGENT,
              deviceInfo.AndroidVer.VersionNumber,deviceInfo.DeviceModelIdentifier,
              $"{deviceInfo.AndroidBoardName}{deviceInfo.DeviceModel}");
        }
        public static bool IsEmpty(this string content)
        {
            return string.IsNullOrEmpty(content);
        }
        public static bool IsNotEmpty(this string content)
        {
            return !string.IsNullOrEmpty(content);
        }
        public static string EncodeList(this long[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }
        public static string EncodeList(this string[] listOfValues, bool appendQuotation = true)
        {
            return EncodeList(listOfValues.ToList(), appendQuotation);
        }
        public static string EncodeList(this List<long> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }
        public static string EncodeList(this List<string> listOfValues, bool appendQuotation = true)
        {
            if (!appendQuotation)
                return string.Join(",", listOfValues);
            var list = new List<string>();
            foreach (var item in listOfValues)
                list.Add(item.Encode());
            return string.Join(",", list);
        }
        public static string Encode(this long content)
        {
            return content.ToString().Encode();
        }
        public static string Encode(this string content)
        {
            return "\"" + content + "\"";
        }

        public static string EncodeRecipients(this long[] recipients)
        {
            return EncodeRecipients(recipients.ToList());
        }
        public static string EncodeRecipients(this List<long> recipients)
        {
            var list = new List<string>();
            foreach (var item in recipients)
                list.Add($"[{item}]");
            return string.Join(",", list);
        }

        public static string EncodeUri(this string data)
        {
            return System.Net.WebUtility.UrlEncode(data);
        }

        public static string GetThreadToken()
        {
            var str = "";
            // 6600286272511816379
            str += Rnd.Next(0, 9);
            str += Rnd.Next(0, 9);
            str += Rnd.Next(1000, 9999);
            str += Rnd.Next(11111, 99999);

            str += Rnd.Next(2222, 6789);

            return $"6600{str}";
        }
        public static string GetJson(this InstaLocationShort location)
        {
            if (location == null)
                return null;

            return new JObject
                            {
                                {"name", location.Address ?? string.Empty},
                                {"address", location.ExternalId ?? string.Empty},
                                {"lat", location.Lat},
                                {"lng", location.Lng},
                                {"external_source", location.ExternalSource ?? "facebook_places"},
                                {"facebook_places_id", location.ExternalId},
                            }.ToString(Formatting.None);
        }

        public static InstaTVChannelType GetChannelType(this string type)
        {
            if(string.IsNullOrEmpty(type))
                return InstaTVChannelType.User;
            switch (type.ToLower())
            {
                case "chrono_following":
                    return InstaTVChannelType.ChronoFollowing;
                case "continue_watching":
                    return InstaTVChannelType.ContinueWatching;
                case "for_you":
                    return InstaTVChannelType.ForYou;
                case "popular":
                    return InstaTVChannelType.Popular;
                default:
                case "user":
                    return InstaTVChannelType.User;
            }
        }
        public static string GetRealChannelType(this InstaTVChannelType type)
        {
            switch(type)
            {
                case InstaTVChannelType.ChronoFollowing:
                    return "chrono_following";
                case InstaTVChannelType.ContinueWatching:
                    return "continue_watching";
                case InstaTVChannelType.Popular:
                    return "popular";
                case InstaTVChannelType.User:
                    return "user";
                case InstaTVChannelType.ForYou:
                default:
                    return "for_you";

            }
        }
        public static string GetChannelDeviceType(this InstaPushChannelType type)
        {
            switch(type)
            {
                default:
                case InstaPushChannelType.Mqtt:
                    return "android_mqtt";
                case InstaPushChannelType.Gcm:
                    return "android_gcm";
            }
        }
        readonly static Random Rnd = new Random();
        public static string GenerateRandomString(this int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[Rnd.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
        
        public static void PrintInDebug(this object obj)
        {
            System.Diagnostics.Debug.WriteLine(Convert.ToString(obj));
        }
        public static string EncodeTime(this TimeSpan span) => $"{span.Hours.ToString("00")}:{span.Minutes.ToString("00")}:{span.Seconds.ToString("00")}";

        public static InstaImageUpload ConvertToImageUpload(this InstaImage instaImage, InstaUserTagUpload[] userTags = null)
        {
            return new InstaImageUpload
            {
                Height = instaImage.Height,
                ImageBytes = instaImage.ImageBytes,
                Uri = instaImage.Uri,
                Width = instaImage.Width,
                UserTags = userTags?.ToList()
            };
        }
        public static InstaComment ConvertToComment(this InstaCommentShort commentShort)
        {
            return new InstaComment
            {
                ContentType = commentShort.ContentType,
                User=  commentShort.User,
                Pk = commentShort.Pk,
                Text = commentShort.Text,
                Type = commentShort.Type,
                CreatedAt = commentShort.CreatedAt,
                CreatedAtUtc = commentShort.CreatedAtUtc,
                HasLikedComment = commentShort.HasLikedComment
            };
        }
        public static JObject ConvertToJson(this InstaStoryPollUpload poll)
        {
            var jArray = new JArray
            {
                new JObject
                {
                    {"text", poll.Answer1},
                    {"count", 0},
                    {"font_size", poll.Answer1FontSize}
                },
                new JObject
                {
                    {"text", poll.Answer2},
                    {"count", 0},
                    {"font_size", poll.Answer2FontSize}
                },
            };

            return new JObject
            {
                {"x", poll.X},
                {"y", poll.Y},
                {"z", poll.Z},
                {"width", poll.Width},
                {"height", poll.Height},
                {"rotation", poll.Rotation},
                {"question", poll.Question},
                {"viewer_vote", 0},
                {"viewer_can_vote", true},
                {"tallies", jArray},
                {"is_shared_result", false},
                {"finished", false},
                {"is_sticker", poll.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStoryLocationUpload location)
        {
            return new JObject
            {
                {"x", location.X},
                {"y", location.Y},
                {"z", location.Z},
                {"width", location.Width},
                {"height", location.Height},
                {"rotation", location.Rotation},
                {"location_id", location.LocationId},
                {"is_sticker", location.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStoryHashtagUpload hashtag)
        {
            return new JObject
            {
                {"x", hashtag.X},
                {"y", hashtag.Y},
                {"z", hashtag.Z},
                {"width", hashtag.Width},
                {"height", hashtag.Height},
                {"rotation", hashtag.Rotation},
                {"tag_name", hashtag.TagName},
                {"is_sticker", hashtag.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStorySliderUpload slider)
        {
            return new JObject
            {
                {"x", slider.X},
                {"y", slider.Y},
                {"z", slider.Z},
                {"width", slider.Width},
                {"height", slider.Height},
                {"rotation", slider.Rotation},
                {"question", slider.Question},
                {"viewer_can_vote", true},
                {"viewer_vote", -1.0},
                {"slider_vote_average", 0.0},
                {"background_color", slider.BackgroundColor},
                {"emoji", $"{slider.Emoji}"},
                {"text_color", slider.TextColor},
                {"is_sticker", slider.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaMediaStoryUpload mediaStory)
        {
            return new JObject
            {
                {"x", mediaStory.X},
                {"y", mediaStory.Y},
                {"width", mediaStory.Width},
                {"height", mediaStory.Height},
                {"rotation", mediaStory.Rotation},
                {"media_id", mediaStory.MediaPk},
                {"is_sticker", mediaStory.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStoryMentionUpload storyMention)
        {
            return new JObject
            {
                {"x", storyMention.X},
                {"y", storyMention.Y},
                {"z", storyMention.Z},
                {"width", storyMention.Width},
                {"height", storyMention.Height},
                {"rotation", storyMention.Rotation},
                {"user_id", storyMention.Pk}
            };
        }

        public static JObject ConvertToJson(this InstaStoryQuestionUpload question)
        {
            return new JObject
            {
                {"x", question.X},
                {"y", question.Y},
                {"z", question.Z},
                {"width", question.Width},
                {"height", question.Height},
                {"rotation", question.Rotation},
                {"question", question.Question},
                {"viewer_can_interact", question.ViewerCanInteract},
                {"profile_pic_url", question.ProfilePicture},
                {"question_type", question.QuestionType},
                {"background_color", question.BackgroundColor},
                {"text_color", question.TextColor},
                {"is_sticker", question.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStoryCountdownUpload countdown)
        {
            return new JObject
            {
                {"x", countdown.X},
                {"y", countdown.Y},
                {"z", countdown.Z},
                {"width", countdown.Width},
                {"height", countdown.Height},
                {"rotation", countdown.Rotation},
                {"text", countdown.Text},
                {"start_background_color", countdown.StartBackgroundColor},
                {"end_background_color", countdown.EndBackgroundColor},
                {"digit_color", countdown.DigitColor},
                {"digit_card_color", countdown.DigitCardColor},
                {"end_ts", countdown.EndTime.ToUnixTime()},
                {"text_color", countdown.TextColor},
                {"following_enabled", countdown.FollowingEnabled},
                {"is_sticker", countdown.IsSticker}
            };
        }

        public static JObject ConvertToJson(this InstaStoryQuizUpload quiz)
        {
            var answers = new JArray();
            if (quiz.Options?.Count > 0)
                foreach (var item in quiz.Options)
                    answers.Add(new JObject
                    {
                        {"text", item.Text},
                        {"count", item.Count}
                    });

            return new JObject
            {
                {"x", quiz.X},
                {"y", quiz.Y},
                {"z", quiz.Z},
                {"width", quiz.Width},
                {"height", quiz.Height},
                {"rotation", quiz.Rotation},
                {"question", quiz.Question},
                {"options", answers},
                {"correct_answer", quiz.CorrectAnswer},
                {"viewer_can_answer", quiz.ViewerCanAnswer},
                {"viewer_answer", quiz.ViewerAnswer},
                {"text_color", quiz.TextColor},
                {"start_background_color", quiz.StartBackgroundColor},
                {"end_background_color", quiz.EndBackgroundColor},
                {"is_sticker", quiz.IsSticker},
            };
        }

        public static JObject ConvertToJson(this InstaStoryChatUpload storyChat)
        {
            return new JObject
            {
                {"x", storyChat.X},
                {"y", storyChat.Y},
                {"z", storyChat.Z},
                {"width", storyChat.Width},
                {"height", storyChat.Height},
                {"rotation", storyChat.Rotation},
                {"text", storyChat.GroupName},
                {"start_background_color", storyChat.StartBackgroundColor},
                {"end_background_color", storyChat.EndBackgroundColor},
                {"has_started_chat", storyChat.HasChatStarted},
                {"is_sticker", storyChat.IsSticker}
            };
        }
    }
}
