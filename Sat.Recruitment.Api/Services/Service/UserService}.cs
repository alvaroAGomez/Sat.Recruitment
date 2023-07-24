﻿using Sat.Recruitment.Api.DTO;
using Sat.Recruitment.Api.Helpers;
using Sat.Recruitment.Api.Models;
using Sat.Recruitment.Api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sat.Recruitment.Api.Services.Service
{
    public class UserService_ : IUser
    {
        public readonly IReadUserFile _readUserFileService;

        public UserService_(IReadUserFile readUserFileService)
        {
            _readUserFileService = readUserFileService;
        }

        public Task<Result> DeleteAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<IList<User>> GetAllAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<User> GetByIdAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result> InsertAndSaveAsync(userCreateDTO user)
        {
            var money = decimal.Parse(user.money);
            var directory = "/Files/Users.txt";
            var newUser = new User
            {
                Name = user.name,
                Email = user.email,
                Address = user.address,
                Phone = user.phone,
                UserType = user.userType,
                Money =  money 
            };
            
            switch (newUser.UserType)
            {
                case "Normal":
                    if ( money  > 100)
                    {
                        var percentage = Convert.ToDecimal(0.12);
                        var gif =  money  * percentage;
                        newUser.Money = newUser.Money + gif;
                    }else
                    {
                        if ( money  > 10)
                        {
                            var percentage = Convert.ToDecimal(0.8);
                            var gif =  money  * percentage;
                            newUser.Money = newUser.Money + gif;
                        }
                    }
                    break;
                case "SuperUser":
                    if ( money  > 100)
                    {
                        var percentage = Convert.ToDecimal(0.20);
                        var gif =  money  * percentage;
                        newUser.Money = newUser.Money + gif;
                    }
                    break;
                case "Premium":
                    if ( money  > 100)
                    {
                        var gif =  money  * 2;
                        newUser.Money = newUser.Money + gif;
                    }
                    break;
                default:
                    break;
            }

            var reader = _readUserFileService.ReadUsersFromFile(directory);

            NormalizeEmail(newUser);

            var usersFromFile = new List<User>();

            while (reader.Peek() >= 0)
            {
                
                var line = reader.ReadLine();
                if(line != "" && line != null) { 
                var userFromFile = ParseUserFromLine(line);
                usersFromFile.Add(userFromFile);
                }
            }
            reader.Close();

            try
            {
                bool isDuplicated = usersFromFile.Any(u => u.Email == newUser.Email || u.Phone == newUser.Phone);

                if (isDuplicated)
                {
                    return new Result()
                    {
                        IsSuccess = false,
                        codeStatus = 400,
                        Errors = "The user is duplicated"
                    };
                }

                bool hasDuplicateNameAndAddress = usersFromFile.Any(u => u.Name == newUser.Name && u.Address == newUser.Address);

                if (hasDuplicateNameAndAddress)
                {
                    return new Result()
                    {
                        IsSuccess = false,
                        codeStatus = 400,
                        Errors = "The user with the same name and address already exists"
                    };
                }


                if (!isDuplicated && !hasDuplicateNameAndAddress)
                {
                   

                    Debug.WriteLine("User Created");

                    if( !_readUserFileService.WriteFileUsers(newUser, directory))
                    {
                        return new Result()
                        {
                            IsSuccess = false,
                            codeStatus = 400,
                            Message = "Could not save user"
                        };
                    }

                    return new Result()
                    {
                        IsSuccess = true,
                        codeStatus = 200,
                        Message = "User Created"
                    };
                }
                else
                {
                    Debug.WriteLine("The user is duplicated");

                    return new Result()
                    {
                        IsSuccess = false,
                        codeStatus = 400,
                        Errors = "The user is duplicated"
                    };
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("The user is duplicated " + e.Message);
                return new Result()
                {
                    IsSuccess = false,
                    codeStatus = 500,
                    Errors = "Server Error" + e.Message
                };
            }

        }

        public Task<Result> UpdateAndSaveAsync(userCreateDTO user, int id)
        {
            throw new System.NotImplementedException();
        }


        private void NormalizeEmail(User user)
        {
            var aux = user.Email.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
            var atIndex = aux[0].IndexOf("+", StringComparison.Ordinal);
            aux[0] = atIndex < 0 ? aux[0].Replace(".", "") : aux[0].Replace(".", "").Remove(atIndex);
            user.Email = string.Join("@", new string[] { aux[0], aux[1] });
        }

        private User ParseUserFromLine(string line)
        {
            var data = line.Split(',');
            return new User
            {
                Name = data[0],
                Email = data[1],
                Phone = data[2],
                Address = data[3],
                UserType = data[4],
                Money = decimal.Parse(data[5]),
            };
        }
    }
}
