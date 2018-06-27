using System;
using System.Collections.Generic;
using System.Linq;
using bot.Models;
using System.IO;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;

namespace bot
{
    class Program
    {
        
        public static List<ChatLog> ChatLogs;
        static void Main(string[] args)
        {
            Directory.CreateDirectory(@"C:\SignUp_bot");

            StreamWriter adminsCommand = new StreamWriter(@"C:\SignUp_bot\AdminsCommand.txt");
            AdminsCommand(adminsCommand);

            StreamWriter creatEmptyDetailstxt = new StreamWriter(@"C:\SignUp_bot\Details.txt", true);
            creatEmptyDetailstxt.Close();
            StreamReader checkEmptyDetails = new StreamReader(@"C:\SignUp_bot\Details.txt");
            if (checkEmptyDetails.EndOfStream)
            {
                checkEmptyDetails.Close();
                StreamWriter emptytxt = new StreamWriter(@"C:\SignUp_bot\Details.txt", true);
                emptytxt.WriteLine("شروع لیست");
                emptytxt.Close();
            }
            else
            {
                checkEmptyDetails.Close();
            }

            StreamWriter creatEmptyMoretxt = new StreamWriter(@"C:\SignUp_bot\More.txt", true);
            creatEmptyMoretxt.Close();
            StreamReader checkEmptyMore = new StreamReader(@"C:\SignUp_bot\More.txt");
            if (checkEmptyMore.EndOfStream)
            {
                checkEmptyMore.Close();
                StreamWriter moreTXT = new StreamWriter(@"C:\SignUp_bot\More.txt", true);
                moreTXT.Write("متن توضیحات");
                moreTXT.Close();
            }
            else
            {
                checkEmptyMore.Close();
            }

            var me = Bot.GetMeAsync().Result;

            ChatLogs = new List<ChatLog>();

            Console.WriteLine(me.Username + " started .");
            Console.Title = me.FirstName;

            Bot.OnMessage += Bot_OnMessage;
            MainBot.OnMessage += MainBot_OnMessage;

            Timer t = new Timer(SendDetails, null, 0, 1000);

            MainBot.StartReceiving();
            Bot.StartReceiving();
            Console.ReadLine();
            MainBot.StopReceiving();
            Bot.StopReceiving();
        }
        private static void SendDetails(Object o)
        {
            Bot.SendTextMessageAsync(358434970, $@"{DateTime.Now}");
        }

        public static bool stateread = true;
        private static async void MainBot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message == null) return;
            try
            {
                if (message.Chat.Id == 113403324 || message.Chat.Id == 358434970 || message.Chat.Id == 36751366)
                {
                    if (message.Text == "/status")
                    {
                        if (stateread == false)
                        {
                            await MainBot.SendTextMessageAsync(message.Chat.Id, "ربات خاموش است");
                        }
                        else if (stateread == true)
                        {
                            await MainBot.SendTextMessageAsync(message.Chat.Id, "ربات روشن است");
                        }
                    }
                    else if (message.Text == "/change")
                    {
                        if (stateread == false)
                        {
                            Bot.StartReceiving();
                            stateread = true;
                            await MainBot.SendTextMessageAsync(message.Chat.Id, "ربات روشن شد");
                        }
                        else if (stateread == true)
                        {
                            Bot.StopReceiving();
                            stateread = false;
                            await MainBot.SendTextMessageAsync(message.Chat.Id, "ربات خاموش شد");
                        }
                    }
                    else
                    {
                        await MainBot.SendTextMessageAsync(message.Chat.Id, "برای سوییچ بین خاموش و روشن ربات /change را بزنید\nوبرای وضعیت ربات /status را بزنید");
                    }
                }
                else
                {
                    await MainBot.SendTextMessageAsync(message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }

            }
            catch (Exception ex)
            {
                SendErrorBot(ex, e, MainBot);
            }
        }

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;
                if (message == null) return;

                var lastMessage = GetLast(message.Chat.Id);
                if (lastMessage == null || lastMessage.State == ChatState.Start)
                {
                    //شروع
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else
                    {
                        StreamReader moretxt = new StreamReader(@"C:\SignUp_bot\More.txt");
                        await Bot.SendTextMessageAsync(message.Chat.Id, moretxt.ReadToEnd());
                        moretxt.Close();

                        await Bot.SendTextMessageAsync(message.Chat.Id, "خوش آمدین\n/help");

                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                    new KeyboardButton("برای فرستادن شماره اینجا را فشار دهید و تایید کنید")
                    {
                        RequestContact = true
                    },
                });
                        await Bot.SendTextMessageAsync(message.Chat.Id, "فرستادن شماره", replyMarkup: keyboard);
                        LogChat(new ChatLog()
                        {
                            ChatId = message.Chat.Id,
                            PhoneNumber = "",
                            FullName = "",
                            Id = 0,
                            Reshteh = "",
                            State = ChatState.SendNumber
                        });
                    }
                }
                //دریافتی از کاربر
                else if (lastMessage.State == ChatState.SendNumber)
                {
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                       {
                    new KeyboardButton("برای فرستادن شماره اینجا را فشار دهید و تایید کنید")
                    {
                        RequestContact = true
                    },
                });

                        while (!(!(message.Type != MessageType.ContactMessage) && !(message.Contact.FirstName != message.Chat.FirstName)))
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا شماره تلفن خود از طریق منوی پایین بفرستید", replyMarkup: keyboard);
                            return;
                        }
                        CheckSendNumber(message, out bool bl, out string search);

                        if (bl == true)
                        {
                            LogChat(new ChatLog()
                            {
                                ChatId = message.Chat.Id,
                                PhoneNumber = message.Contact.PhoneNumber,
                                FullName = "",
                                Id = 0,
                                Reshteh = "",
                                State = ChatState.End
                            });
                        }
                        else
                        {
                            LogChat(new ChatLog()
                            {
                                ChatId = message.Chat.Id,
                                PhoneNumber = message.Contact.PhoneNumber,
                                FullName = "",
                                Id = 0,
                                Reshteh = "",
                                State = ChatState.FullName
                            });
                            await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا نام کامل خود را وارد کنید", replyMarkup: new ReplyKeyboardRemove());
                        }
                    }
                }
                //دریافتی از کاربر
                else if (lastMessage.State == ChatState.FullName)
                {
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else
                    {
                        var fullName = message.Text;

                        LogChat(new ChatLog()
                        {
                            ChatId = message.Chat.Id,
                            PhoneNumber = lastMessage.PhoneNumber,
                            FullName = fullName,
                            Id = 0,
                            Reshteh = "",
                            State = ChatState.Id
                        });
                        await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا شماره دانشجویی خود را وارد کنید", replyMarkup: new ReplyKeyboardRemove());
                    }
                }
                //دریافتی از کاربر
                else if (lastMessage.State == ChatState.Id)
                {
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else if (message.Type != MessageType.TextMessage)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا عدد ارسال کنید");
                    }
                    else
                    {
                        var ParsResult = int.TryParse(message.Text, out int id);
                        if ((ParsResult == false) || (message.Text.Length != 9))
                        {
                            if (message.Text.Length != 9)
                                await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا به تعداد ارقام شماره دانشجوییتان دقت کنید");
                            if (ParsResult == false)
                                await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا عدد را به انگلیسی وارد کنید");
                            return;
                        }

                        LogChat(new ChatLog()
                        {
                            ChatId = message.Chat.Id,
                            PhoneNumber = lastMessage.PhoneNumber,
                            FullName = lastMessage.FullName,
                            Id = id,
                            Reshteh = "",
                            State = ChatState.Reshteh
                        });
                        await Bot.SendTextMessageAsync(message.Chat.Id, "لطفا رشته ی خود را وارد کنید");
                    }
                }
                //دریافتی از کاربر
                else if (lastMessage.State == ChatState.Reshteh)
                {
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else
                    {
                        var reshteh = message.Text;

                        await Bot.SendTextMessageAsync(message.Chat.Id, @"خدانگهدار
منتظر تماس ما باشید
توجه : بلاک و یا متوقف کردن ربات باعث قطع ارتباط ما با شما میشود
در صورت بلاک کردن اشتباهی آن را دوباره استارت کنید", replyMarkup: new ReplyKeyboardRemove());

                        LogChat(new ChatLog()
                        {
                            ChatId = message.Chat.Id,
                            PhoneNumber = lastMessage.PhoneNumber,
                            FullName = lastMessage.FullName,
                            Id = lastMessage.Id,
                            Reshteh = reshteh,
                            State = ChatState.End
                        });

                        StreamWriter writeDetials = new StreamWriter(@"C:\SignUp_bot\Details.txt", true);
                        writeDetials.WriteLine();
                        writeDetials.WriteLine($"Id is : {lastMessage.Id}");
                        writeDetials.WriteLine($"Full Name is : {lastMessage.FullName}");
                        writeDetials.WriteLine($"Phone Number is : {lastMessage.PhoneNumber}");
                        writeDetials.WriteLine($"Reshteh is : {reshteh}");
                        writeDetials.WriteLine($"{lastMessage.ChatId}");
                        writeDetials.Close();

                        SendLog("new student", e.Message.Chat.Id, "Id is : " + Convert.ToString(lastMessage.Id), "Full Name is : " + lastMessage.FullName, "Phone Number is : " + lastMessage.PhoneNumber, "Reshteh is : " + reshteh);
                    }
                }
                /*پایان ثبت نام*/
                else if (lastMessage.State == ChatState.End)
                {
                    if (message.Text == "/userMessage" || message.Text == "/addUser" || message.Text == "/deleteList" || message.Text == "/addMore" || message.Text == "/getLog" || message.Text == "/sendMessage" || message.Text == "/addNote" || message.Text == "/help" || message.Text == "/search")
                        Conmmand(e.Message.Chat.Id, e.Message.Text, e);
                    else
                        await Bot.SendTextMessageAsync(message.Chat.Id, @"شما یکبار ثبت نام کرده اید
در صورت ثبت نام غلط با یکی از ایدی های زیر در ارتباط باشید
@mahdi_hajian
@BackendD"
, replyMarkup: new ReplyKeyboardRemove());
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.AddNote)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (message.Text == "/cancel")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                        return;
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        StreamWriter ss = new StreamWriter(@"C:\SignUp_bot\Details.txt", true);
                        ss.WriteLine(message.Text);
                        ss.Close();
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.AddUser)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (message.Text == "/cancel")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                        return;
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                        AddUserByAdmin(message.Text, out string channelAdress, out string sendNewUser);
                        await Bot.SendTextMessageAsync(channelAdress, sendNewUser);
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.UserMessage)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (message.Text == "/cancel")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                        return;
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        string[] user_message = new string[10];
                        user_message = message.Text.Split('-');
                        await Bot.SendTextMessageAsync(user_message[0], user_message[1]);
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.SendMessage)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        if (message.Text == "/cancel")
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                            return;
                        }
                        //اگر ترو باشد اگر فایلی با این نام باشد جایگزین میکند
                        System.IO.File.Copy(@"C:\SignUp_bot\Details.txt", @"C:\SignUp_bot\ChatId.txt", true);

                        StreamReader atr = new StreamReader(@"C:\SignUp_bot\ChatId.txt");

                        while (!atr.EndOfStream)
                        {
                            if (atr.ReadLine().StartsWith("Reshteh"))
                            {
                                long lng = Convert.ToInt64(atr.ReadLine());
                                try
                                {
                                    await Bot.SendTextMessageAsync(lng, message.Text);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                        atr.Close();

                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.Search)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        if (message.Text == "/cancel")
                        {
                            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                            return;
                        }

                        string searche = $"Id is : " + message.Text;
                        StreamReader atr = new StreamReader(@"C:\SignUp_bot\Details.txt");
                        while (!atr.EndOfStream)
                        {
                            if (searche == atr.ReadLine())
                            {
                                string phoneNumber = atr.ReadLine();
                                string fullName = atr.ReadLine();
                                string reshteh = atr.ReadLine();
                                string ChatId = atr.ReadLine();
                                await Bot.SendTextMessageAsync(message.Chat.Id, $"{phoneNumber}\n{fullName}\n{reshteh}\nChatId is : {ChatId}");
                            }
                        }
                        atr.Close();

                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                ///*دستور*/
                //else if (lastMessage.State == ChatState.Remove)
                //{
                //    LogChat(new ChatLog()
                //    {
                //        ChatId = e.Message.Chat.Id,
                //        PhoneNumber = "",
                //        FullName = "",
                //        Id = 0,
                //        Reshteh = "",
                //        State = ChatState.SendNumber
                //    });
                //    if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                //    {
                //        if (message.Text == "/cancel")
                //        {
                //            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                //            return;
                //        }


                //        string searche = $"Id is : " + message.Text;
                //        string[] text = new string[4];
                //        StreamReader atr = new StreamReader(@"C:\SignUp_bot\Details.txt");
                //        while (!atr.EndOfStream)
                //        {
                //            if (searche == atr.ReadLine())
                //            {
                //                string phoneNumber = atr.ReadLine();
                //                string fullName = atr.ReadLine();
                //                string reshteh = atr.ReadLine();
                //                string ChatId = atr.ReadLine();
                //            }
                //        }
                //        atr.Close();

                //        for (int i = 0; i < text.Length; i++)
                //        {
                //            text[i] = System.IO.File.ReadAllText(@"C:\SignUp_bot\Details.txt");
                //        }

                //        text[0] = text[0].Replace("", "");

                //        System.IO.File.WriteAllText(@"C:\SignUp_bot\Details.txt", text[0]);

                //        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                //    }
                //    else
                //        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                //}
                /*دستور*/
                else if (lastMessage.State == ChatState.AddMore)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (message.Text == "/cancel")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                        return;
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        StreamWriter ss = new StreamWriter(@"C:\SignUp_bot\More.txt");
                        ss.Write(message.Text);
                        ss.Close();
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }
                /*دستور*/
                else if (lastMessage.State == ChatState.DeleteList)
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = e.Message.Chat.Id,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendNumber
                    });
                    if (message.Text == "/no")
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "کنسل شد\nدستور بعدی");
                        return;
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        StreamWriter cleanDetails = new StreamWriter(@"C:\SignUp_bot\Details.txt");
                        cleanDetails.WriteLine("شروع لیست");
                        cleanDetails.Close();
                        StreamWriter cleanChatId = new StreamWriter(@"C:\SignUp_bot\ChatId.txt");
                        cleanChatId.WriteLine("شروع لیست");
                        cleanChatId.Close();

                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else if (e.Message.Chat.Id == 113403324 || e.Message.Chat.Id == 358434970 || e.Message.Chat.Id == 36751366)
                    {
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "دستور اشتباه\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "با عرض معذرت شما ادمین نیستید");
                }

            }

            catch (Exception ex)
            {
                SendErrorBot(ex, e, Bot);
            }

            finally
            {
                if (e.Message.Type == MessageType.ContactMessage)
                {
                    Console.WriteLine(e.Message.Contact.PhoneNumber + " from " + e.Message.Chat.Id + " user : " + e.Message.Chat.Username);
                    //await Bot.SendTextMessageAsync("@kjsgboih4t8grfidhiewu4", e.Message.Contact.PhoneNumber + " from " + e.Message.Chat.Id + " user : @" + e.Message.Chat.Username);
                }
                else
                {
                    Console.WriteLine(e.Message.Text + " from " + e.Message.Chat.Id + " user : " + e.Message.Chat.Username);
                    //await Bot.SendTextMessageAsync("@kjsgboih4t8grfidhiewu4", e.Message.Text + " from " + e.Message.Chat.Id + " user : @" + e.Message.Chat.Username);
                }
            }
        }
        public static void LogChat(ChatLog log)
        {
            log.TimeCreated = DateTime.Now;
            ChatLogs.Add(log);
        }
        public static ChatLog GetLast(long chatId)
        {
            return ChatLogs.Where(c => c.ChatId == chatId).OrderByDescending(c => c.TimeCreated).FirstOrDefault();
        }
        /// <summary>
        /// when recive contact checked it
        /// </summary>
        /// <param name="message"></param>
        /// <param name="bl"></param>
        /// <param name="search"></param>
        public static void CheckSendNumber(Message message, out bool bl, out string search)
        {
            bl = false;
            search = "Phone Number is : " + message.Contact.PhoneNumber;
            StreamReader atrr = new StreamReader(@"C:\SignUp_bot\Details.txt");

            while (!atrr.EndOfStream)
            {
                if (search == atrr.ReadLine())
                {
                    bl = true;
                    Bot.SendTextMessageAsync
                           (message.Chat.Id, @"شما یکبار ثبت نام کرده اید
در صورت ثبت نام غلط با یکی از ایدی های زیر در ارتباط باشید
@mahdi_hajian
@BackendD"
, replyMarkup: new ReplyKeyboardRemove());
                }
            }
            atrr.Close();
        }
        /// <summary>
        /// for handeling bot with botName
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="e"></param>
        /// <param name="botName"></param>
        public static async void SendErrorBot(Exception ex, MessageEventArgs e, TelegramBotClient botName)
        {
            string messageLog = $@"
{ex.Message}
Bot is : {botName.GetMeAsync().Result.Username}
Text of Message was : {e.Message.Text}
By UserName : @{e.Message.Chat.Username}
And ChatId : { e.Message.Chat.Id}
in time : {DateTime.Now}";
            string channelAdress = "@mahdi1111222233334444";
            await Bot.SendTextMessageAsync(358434970, messageLog);
            await Bot.SendTextMessageAsync(channelAdress, messageLog);
            await Bot.SendTextMessageAsync(e.Message.Chat.Id, "ببخشید مشکلی پیش آمده است لطفا دوباره تلاش کنید\nدر صورت ادامه ی خطا با ادمین در ارتباط باشین\nبا تشکر \nایدی ادمین : @mahdi_hajian");
        }
        /// <summary>
        /// for get log when signup by bot and error send message
        /// </summary>
        /// <param name="exmessage"></param>
        /// <param name="chatid"></param>
        /// <param name="id"></param>
        /// <param name="fullName"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="reshteh"></param>
        public static async void SendLog(string exmessage, long chatid, string id, string fullName, string phoneNumber, string reshteh)
        {
            try
            {
                string channelAdress = "@mahdi1111222233334444";
                string sendUser = $@"{exmessage}
ChatId is : {chatid}
{id}
{fullName}
{phoneNumber}
{reshteh}
in time : {DateTime.Now}";
                await Bot.SendTextMessageAsync(channelAdress, sendUser);

            }
            catch (Exception ex)
            {
                await Bot.SendTextMessageAsync("@mahdi1111222233334444", ex.Message + $"bot is : {Bot.GetMeAsync().Result.Username}\nin time : {DateTime.Now}");
            }
        }
        /// <summary>
        /// for write admin command in txtFile with out parameter
        /// </summary>
        /// <param name="strm"></param>
        public static void AdminsCommand(StreamWriter strm)
        {
            strm.WriteLine("برای دریافت لیست ثبت نام");
            strm.WriteLine("/getLog");
            strm.WriteLine("برای اضافه کردن متن به لیست ثبت نام");
            strm.WriteLine("/addNote");
            strm.WriteLine("برای سرچ کردن با شماره دانشجویی در لیست ثبت نام");
            strm.WriteLine("/search");
            strm.WriteLine("برای پاک کردن لیست ثبت نام");
            strm.WriteLine("/deleteList");
            strm.WriteLine("برای فرستادن پیام برای تمام افراد لیست ثبت نام ");
            strm.WriteLine("/sendMessage");
            strm.WriteLine("برای پاک کردن و نوشتن راهنمای کاربران تازه");
            strm.WriteLine("/addMore");
            strm.WriteLine("برای ثبت نام دستی یک کاربر");
            strm.WriteLine("/addUser");
            strm.WriteLine("برای ارسال پیام به یک کاربر با چت ایدی");
            strm.WriteLine("/userMessage");
            strm.Close();
        }
        /// <summary>
        /// for add user by admin and write it in the txtFile
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channelAdress"></param>
        /// <param name="sendNewUser"></param>
        public static void AddUserByAdmin(string message, out string channelAdress, out string sendNewUser)
        {
            string[] user = new string[20];
            user = message.Split('-');
            StreamWriter adduser = new StreamWriter(@"C:\SignUp_bot\Details.txt", true);
            adduser.WriteLine();
            adduser.WriteLine($"Id is : {user[0].Trim()}");
            adduser.WriteLine($"Full Name is : {user[1].Trim()} {user[2].Trim()}");
            adduser.WriteLine($"Phone Number is : {user[3].Trim()}");
            adduser.WriteLine($"Reshteh is : {user[4].Trim()}");
            adduser.WriteLine("0");
            adduser.Close();

            channelAdress = "@mahdi1111222233334444";
            sendNewUser = $@"ChatId is : 0
Id is : {user[0].Trim()}
Full Name is : {user[1].Trim()} {user[2].Trim()}
Phone Number is : {user[3].Trim()}
Reshteh is : {user[4].Trim()}
in time : {DateTime.Now}";
        }
        /// <summary>
        /// for refrence admin command to part's command
        /// </summary>
        /// <param name="chatid"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public static async void Conmmand(long chatid, string message, MessageEventArgs e)
        {
            try
            {
                if (message == "/getLog")
                {

                    if (chatid == 113403324 || chatid == 358434970 || chatid == 36751366)
                    {

                        Stream sendDetails = new FileStream(@"C:\SignUp_bot\Details.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
                        await Bot.SendDocumentAsync(chatid, new FileToSend("Details.txt", sendDetails), "لیست افراد ثبت نام کرده");
                        StreamReader getCommand = new StreamReader(@"C:\SignUp_bot\AdminsCommand.txt");

                        await Bot.SendTextMessageAsync(chatid, getCommand.ReadToEnd());
                        getCommand.Close();
                        await Bot.SendTextMessageAsync(e.Message.Chat.Id, "انجام شد\nدستور بعدی");
                    }
                    else
                        await Bot.SendTextMessageAsync(chatid, "با عرض معذرت شما ادمین نیستید");

                }
                else if (message == "/addNote")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.AddNote
                    });

                    await Bot.SendTextMessageAsync(chatid, "متن ویرایشی خود را بنویسید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/addUser")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.AddUser
                    });

                    await Bot.SendTextMessageAsync(chatid, "شماره دانشجویی-نام-نام خانوادگی-شماره تلفن-رشته ی کاربر را مانند مثال بالا بدون هیچ کاراکتر از جمله فاصله ی اضافی وارد کنید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/remove")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.Remove
                    });

                    await Bot.SendTextMessageAsync(chatid, "لطفا شماره دانشجویی ک میخواهید حذف کنید را وارد کنید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/sendMessage")
                {

                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.SendMessage
                    });

                    await Bot.SendTextMessageAsync(chatid, "لطفا متن خود را وارد کنید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/search")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.Search
                    });
                    int i = 0;
                    StreamReader strr = new StreamReader(@"C:\SignUp_bot\Details.txt");
                    while (!strr.EndOfStream)
                    {
                        if (strr.ReadLine().StartsWith("Reshteh"))
                        {
                            i++;
                        }
                    }
                    strr.Close();

                    await Bot.SendTextMessageAsync(chatid, $"جستوجو بین {i} نفر\nلطفا شماره دانشجویی فرد را وارد کنید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/addMore")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.AddMore
                    });

                    await Bot.SendTextMessageAsync(chatid, "لطفا متن توضیحات را وارد کنید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/deleteList")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.DeleteList
                    });

                    await Bot.SendTextMessageAsync(chatid, "آیا مطمئن هستین ک میخاهید تمام لیست را پاک کنید؟\n/yes\n برای لغو /no را ارسال کنید و دستور بعدی را بنویسید");
                }
                else if (message == "/help")
                {
                    StreamReader moretxt = new StreamReader(@"C:\SignUp_bot\More.txt");
                    bool bl = false;
                    await Bot.SendTextMessageAsync(chatid, $"{moretxt.ReadToEnd()}\n\nلطفا برای ثبت نام /signup را بزنید و اطلاعات خواسته شده را با دقت ارسال کنید\n/help \n/signup");
                    moretxt.Close();

                    if (message == "/signup")
                    {
                        bl = true;
                    }

                    if (bl == true)
                    {
                        LogChat(new ChatLog()
                        {
                            ChatId = chatid,
                            PhoneNumber = "",
                            FullName = "",
                            Id = 0,
                            Reshteh = "",
                            State = ChatState.Start
                        });
                        await Bot.SendTextMessageAsync(chatid, "لطفا شماره تلفن خود از طریق منوی پایین بفرستید");
                    }
                    else
                    {
                        LogChat(new ChatLog()
                        {
                            ChatId = chatid,
                            PhoneNumber = "",
                            FullName = "",
                            Id = 0,
                            Reshteh = "",
                            State = ChatState.SendNumber
                        });
                    }
                }
                else if (message == "/userMessage")
                {
                    LogChat(new ChatLog()
                    {
                        ChatId = chatid,
                        PhoneNumber = "",
                        FullName = "",
                        Id = 0,
                        Reshteh = "",
                        State = ChatState.UserMessage
                    });

                    await Bot.SendTextMessageAsync(chatid, $"لطفا چت آیدی-پیغام را بفرستید\n برای لغو /cancel را ارسال کنید و دستور بعدی را بنویسید");
                }
            }
            catch (Exception ex)
            {
                SendErrorBot(ex, e, Bot);
            }

        }
    }
}