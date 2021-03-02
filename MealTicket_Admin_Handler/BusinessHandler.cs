using MealTicket_Admin_Handler.Model;
using Newtonsoft.Json;
using SMS_SendHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Admin_Handler
{
    public class BusinessHandler
    {
        #region====页面管理====
        /// <summary>
        /// 查询页面配置列表
        /// </summary>
        /// <returns></returns>
        public PageRes<PageSettingInfo> GetPageSettingList(GetPageSettingListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var pageSetting = from item in db.t_page_setting
                                  select item;
                if (!string.IsNullOrEmpty(request.PageCode))
                {
                    pageSetting = from item in pageSetting
                                  where item.PageCode.Contains(request.PageCode)
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    pageSetting = from item in pageSetting
                                  where item.Id <= request.MaxId
                                  select item;
                }
                int totalCount = pageSetting.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = pageSetting.Max(e => e.Id);
                }

                return new PageRes<PageSettingInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in pageSetting
                            orderby item.CreateTime descending
                            select new PageSettingInfo
                            {
                                Status = item.Status,
                                CreateTime = item.LastModified,
                                Id = item.Id,
                                PageCode = item.PageCode,
                                PageName = item.PageName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 修改页面配置状态
        /// </summary>
        public void ModifyPageSettingStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var pageSetting = (from item in db.t_page_setting
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (pageSetting == null)
                {
                    throw new WebApiException(400, "配置不存在");
                }
                pageSetting.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询页面配置详细信息
        /// </summary>
        /// <returns></returns>
        public PageSettingDetails GetPageSettingDetails(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var pageSetting = (from item in db.t_page_setting
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (pageSetting == null)
                {
                    throw new WebApiException(400, "配置不存在");
                }
                return new PageSettingDetails
                {
                    Content = pageSetting.Content
                };
            }
        }

        /// <summary>
        /// 保存页面配置
        /// </summary>
        public void SavePageSetting(SavePageSettingRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var pageSetting = (from item in db.t_page_setting
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (pageSetting == null)
                {
                    throw new WebApiException(400, "配置不存在");
                }
                pageSetting.Content = request.Content;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询客服配置列表
        /// </summary>
        /// <returns></returns>
        public PageRes<CustomerServiceSettingInfo> GetCustomerServiceSettingList(GetCustomerServiceSettingListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = from item in db.t_customerservice_setting
                              select item;
                if (!string.IsNullOrEmpty(request.PageCode))
                {
                    setting = from item in setting
                              where item.PageCode.Contains(request.PageCode)
                              select item;
                }
                if (request.MaxId > 0)
                {
                    setting = from item in setting
                              where item.Id <= request.MaxId
                              select item;
                }
                int totalCount = setting.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = setting.Max(e => e.Id);
                }

                return new PageRes<CustomerServiceSettingInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in setting
                            orderby item.CreateTime descending
                            select new CustomerServiceSettingInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                PageCode = item.PageCode,
                                PageName = item.PageName,
                                ImgUrl = item.ImgUrl,
                                WechatNumber = item.WechatNumber,
                                Mobile = item.Mobile
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加客服配置
        /// </summary>
        public void AddCustomerServiceSetting(AddCustomerServiceSettingRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断页面code是否存在
                var setting = (from item in db.t_customerservice_setting
                               where item.PageCode == request.PageCode
                               select item).FirstOrDefault();
                if (setting != null)
                {
                    throw new WebApiException(400, "该页面已添加");
                }
                db.t_customerservice_setting.Add(new t_customerservice_setting
                {
                    Status = 1,
                    CreateTime = DateTime.Now,
                    ImgUrl = request.ImgUrl,
                    LastModified = DateTime.Now,
                    Mobile = request.Mobile,
                    PageCode = request.PageCode,
                    PageName = request.PageName,
                    WechatNumber = request.WechatNumber
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑客服配置
        /// </summary>
        public void ModifyCustomerServiceSetting(ModifyCustomerServiceSettingRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = (from item in db.t_customerservice_setting
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (setting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                setting.LastModified = DateTime.Now;
                setting.ImgUrl = request.ImgUrl;
                setting.WechatNumber = request.WechatNumber;
                setting.Mobile = request.Mobile;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改客服配置状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyCustomerServiceSettingStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = (from item in db.t_customerservice_setting
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (setting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                setting.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除客服配置
        /// </summary>
        /// <param name="request"></param>
        public void DeleteCustomerServiceSetting(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var setting = (from item in db.t_customerservice_setting
                               where item.Id == request.Id
                               select item).FirstOrDefault();
                if (setting == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_customerservice_setting.Remove(setting);
                db.SaveChanges();
            }
        }
        #endregion

        #region====帮助中心====
        /// <summary>
        /// 查询帮助中心问题分类列表
        /// </summary>
        /// <returns></returns>
        public PageRes<HelpcenterQuestionTypeInfo> GetHelpcenterQuestionTypeList(GetHelpcenterQuestionTypeListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var questionType = from item in db.t_helpcenter_question_type
                                   select item;
                if (!string.IsNullOrEmpty(request.TypeName))
                {
                    questionType = from item in questionType
                                   where item.TypeName.Contains(request.TypeName)
                                   select item;
                }
                if (request.MaxId > 0)
                {
                    questionType = from item in questionType
                                   where item.Id <= request.MaxId
                                   select item;
                }
                int totalCount = questionType.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = questionType.Max(e => e.Id);
                }

                List<long> typeIdList = new List<long>();
                if (request.QuestionId > 0)
                {
                    typeIdList = (from item in db.t_helpcenter_question_type_content_rel
                                  where item.ContentId == request.QuestionId
                                  select item.TypeId).Distinct().ToList();
                }
                return new PageRes<HelpcenterQuestionTypeInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in questionType
                            orderby item.CreateTime descending
                            select new HelpcenterQuestionTypeInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                OrderIndex = item.OrderIndex,
                                TypeName = item.TypeName,
                                IconUrl = item.IconUrl,
                                IsCheck = typeIdList.Contains(item.Id) ? true : false
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加帮助中心问题分类
        /// </summary>
        public void AddHelpcenterQuestionType(AddHelpcenterQuestionTypeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var questionType = (from item in db.t_helpcenter_question_type
                                    where item.TypeName == request.TypeName
                                    select item).FirstOrDefault();
                if (questionType != null)
                {
                    throw new WebApiException(400, "分类名已存在");
                }

                db.t_helpcenter_question_type.Add(new t_helpcenter_question_type
                {
                    CreateTime = DateTime.Now,
                    IconUrl = request.IconUrl,
                    OrderIndex = request.OrderIndex,
                    TypeName = request.TypeName,
                    UpdateTime = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑帮助中心问题分类
        /// </summary>
        /// <param name="request"></param>
        public void ModifyHelpcenterQuestionType(ModifyHelpcenterQuestionTypeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var questionType = (from item in db.t_helpcenter_question_type
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (questionType == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                var temp = (from item in db.t_helpcenter_question_type
                            where item.TypeName == request.TypeName && item.Id != request.Id
                            select item).FirstOrDefault();
                if (temp != null)
                {
                    throw new WebApiException(400, "分类名已存在");
                }
                questionType.TypeName = request.TypeName;
                questionType.IconUrl = request.IconUrl;
                questionType.UpdateTime = DateTime.Now;
                questionType.OrderIndex = request.OrderIndex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除帮助中心问题分类
        /// </summary>
        /// <param name="request"></param>
        public void DeleteHelpcenterQuestionType(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var questionType = (from item in db.t_helpcenter_question_type
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (questionType == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }

                db.t_helpcenter_question_type.Remove(questionType);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询帮助中心问题列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<HelpcenterQuestionInfo> GetHelpcenterQuestionList(GetHelpcenterQuestionListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = from item in db.t_helpcenter_question_content
                           select item;
                if (!string.IsNullOrEmpty(request.QuestionName))
                {
                    temp = from item in temp
                           where item.Name.Contains(request.QuestionName)
                           select item;
                }
                if (request.MaxId > 0)
                {
                    temp = from item in temp
                           where item.Id <= request.MaxId
                           select item;
                }
                int totalCount = temp.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = temp.Max(e => e.Id);
                }

                var list = (from item in temp
                            orderby item.CreateTime descending
                            select new HelpcenterQuestionInfo
                            {
                                Status = item.Status,
                                Answer = item.Answer,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                IsCommon = item.IsCommon,
                                IsTop = item.IsTop,
                                Name = item.Name,
                                OrderIndex = item.OrderIndex,
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in list)
                {
                    var typeList = (from x in db.t_helpcenter_question_type_content_rel
                                    join x2 in db.t_helpcenter_question_type on x.TypeId equals x2.Id
                                    where x.ContentId == item.Id
                                    select x2.TypeName).ToList();
                    item.Type = string.Join("/", typeList.ToArray());
                }

                return new PageRes<HelpcenterQuestionInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 添加帮助中心问题
        /// </summary>
        public void AddHelpcenterQuestion(AddHelpcenterQuestionRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_helpcenter_question_content.Add(new t_helpcenter_question_content
                {
                    Status = 1,
                    Answer = request.Answer,
                    CreateTime = DateTime.Now,
                    IsCommon = false,
                    IsTop = false,
                    Name = request.Name,
                    OrderIndex = request.OrderIndex,
                    UpdateTime = DateTime.Now
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑帮助中心问题
        /// </summary>
        public void ModifyHelpcenterQuestion(ModifyHelpcenterQuestionRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题不存在");
                }
                question.Name = request.Name;
                question.Answer = request.Answer;
                question.OrderIndex = request.OrderIndex;
                question.UpdateTime = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改帮助中心问题状态
        /// </summary>
        public void ModifyHelpcenterQuestionStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题不存在");
                }
                question.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改帮助中心问题是否置顶
        /// </summary>
        public void ModifyHelpcenterQuestionIsTop(ModifyHelpcenterQuestionIsTopRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题不存在");
                }
                question.IsTop = request.IsTop;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改帮助中心问题是否常见问题
        /// </summary>
        public void ModifyHelpcenterQuestionIsCommon(ModifyHelpcenterQuestionIsCommonRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题不存在");
                }
                question.IsCommon = request.IsCommon;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 绑定问题所属分类
        /// </summary>
        public void BindHelpcenterQuestionType(BindHelpcenterQuestionTypeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.QuestionId
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题数据不存在");
                }
                var oldTypeRel = (from item in db.t_helpcenter_question_type_content_rel
                                  where item.ContentId == request.QuestionId
                                  select item).ToList();
                if (oldTypeRel.Count() > 0)
                {
                    db.t_helpcenter_question_type_content_rel.RemoveRange(oldTypeRel);
                }

                var typeId = (from item in db.t_helpcenter_question_type.AsNoTracking()
                              where request.TypeId.Contains(item.Id)
                              select item.Id).ToList();
                foreach (var item in typeId)
                {
                    db.t_helpcenter_question_type_content_rel.Add(new t_helpcenter_question_type_content_rel
                    {
                        ContentId = request.QuestionId,
                        CreateTime = DateTime.Now,
                        TypeId = item
                    });
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除帮助中心问题
        /// </summary>
        /// <param name="request"></param>
        public void DeleteHelpcenterQuestion(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = (from item in db.t_helpcenter_question_content
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (question == null)
                {
                    throw new WebApiException(400, "问题不存在");
                }
                db.t_helpcenter_question_content.Remove(question);
                db.SaveChanges();
            }
        }
        #endregion

        #region====广告管理====
        /// <summary>
        /// 查询banner广告分组列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BannerGroupInfo> GetBannerGroupList(GetBannerGroupListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var bannerGroup = from item in db.t_banner_group
                                  select item;
                if (!string.IsNullOrEmpty(request.GroupCode))
                {
                    bannerGroup = from item in bannerGroup
                                  where item.GroupCode.Contains(request.GroupCode)
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    bannerGroup = from item in bannerGroup
                                  where item.Id <= request.MaxId
                                  select item;
                }
                int totalCount = bannerGroup.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = bannerGroup.Max(e => e.Id);
                }

                return new PageRes<BannerGroupInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in bannerGroup
                            orderby item.CreateTime descending
                            select new BannerGroupInfo
                            {
                                Status = item.Status,
                                CreateTime = item.LastModified,
                                Id = item.Id,
                                GroupCode = item.GroupCode,
                                GroupDes = item.GroupDes,
                                BannerCount = (from x in db.t_banner
                                               where x.GroupId == item.Id
                                               select x).Count()
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 编辑banner广告分组
        /// </summary>
        public void ModifyBannerGroup(ModifyBannerGroupRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var bannerGroup = (from item in db.t_banner_group
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (bannerGroup == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }
                bannerGroup.GroupDes = request.GroupDes;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改banner广告分组状态
        /// </summary>
        public void ModifyBannerGroupStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var bannerGroup = (from item in db.t_banner_group
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (bannerGroup == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }
                bannerGroup.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询banner广告列表
        /// </summary>
        /// <returns></returns>
        public PageRes<BannerInfo> GetBannerList(GetBannerListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var banner = from item in db.t_banner
                             where item.GroupId == request.GroupId
                             select item;
                if (!string.IsNullOrEmpty(request.BannerName))
                {
                    banner = from item in banner
                             where item.Name.Contains(request.BannerName)
                             select item;
                }
                if (request.MaxId > 0)
                {
                    banner = from item in banner
                             where item.Id <= request.MaxId
                             select item;
                }
                int totalCount = banner.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = banner.Max(e => e.Id);
                }

                return new PageRes<BannerInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in banner
                            orderby item.CreateTime descending
                            select new BannerInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                ActionPath = item.ImgPath,
                                ActionType = item.ActionType,
                                ImgUrl = item.ImgUrl,
                                OrderIndex = item.OrderIndex,
                                Name = item.Name
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加banner广告
        /// </summary>
        public void AddBanner(AddBannerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组id是否处在
                var bannerGroup = (from item in db.t_banner_group
                                   where item.Id == request.GroupId
                                   select item).FirstOrDefault();
                if (bannerGroup == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_banner.Add(new t_banner
                {
                    Status = 1,
                    ActionType = request.ActionType,
                    CreateTime = DateTime.Now,
                    GroupId = request.GroupId,
                    ImgPath = request.ActionPath,
                    ImgUrl = request.ImgUrl,
                    LastModified = DateTime.Now,
                    Name = request.Name,
                    OrderIndex = request.OrderIndex
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑banner广告
        /// </summary>
        public void ModifyBanner(ModifyBannerRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组id是否处在
                var banner = (from item in db.t_banner
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (banner == null)
                {
                    throw new WebApiException(400, "广告不存在");
                }
                banner.ActionType = request.ActionType;
                banner.ImgPath = request.ActionPath;
                banner.ImgUrl = request.ImgUrl;
                banner.LastModified = DateTime.Now;
                banner.Name = request.Name;
                banner.OrderIndex = request.OrderIndex;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改banner广告状态
        /// </summary>
        public void ModifyBannerStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组id是否处在
                var banner = (from item in db.t_banner
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (banner == null)
                {
                    throw new WebApiException(400, "广告不存在");
                }
                banner.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除banner广告
        /// </summary>
        public void DeleteBanner(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组id是否处在
                var banner = (from item in db.t_banner
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (banner == null)
                {
                    throw new WebApiException(400, "广告不存在");
                }
                db.t_banner.Remove(banner);
                db.SaveChanges();
            }
        }
        #endregion

        #region====通知管理====
        /// <summary>
        /// 查询头条列表
        /// </summary>
        /// <returns></returns>
        public PageRes<InformationInfo> GetInformationList(GetInformationListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var information = from item in db.t_information
                                  select item;
                if (!string.IsNullOrEmpty(request.Title))
                {
                    information = from item in information
                                  where item.Title.Contains(request.Title)
                                  select item;
                }
                if (request.MaxId > 0)
                {
                    information = from item in information
                                  where item.Id <= request.MaxId
                                  select item;
                }
                int totalCount = information.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = information.Max(e => e.Id);
                }

                return new PageRes<InformationInfo>
                {
                    MaxId = maxId,
                    TotalCount = totalCount,
                    List = (from item in information
                            orderby item.CreateTime descending
                            select new InformationInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                StartShowTime = item.StartShowTime,
                                EndShowTime = item.EndShowTime,
                                Content = item.Content,
                                ContentIntroduction = item.ContentIntroduction,
                                Title = item.Title
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加头条
        /// </summary>添加头条
        public void AddInformation(AddInformationRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_information.Add(new t_information
                {
                    StartShowTime = request.StartShowTime,
                    EndShowTime = request.EndShowTime,
                    Content = request.Content,
                    ContentIntroduction = request.ContentIntroduction,
                    Status = 1,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    Title = request.Title
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑头条
        /// </summary>
        public void ModifyInformation(ModifyInformationRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var information = (from item in db.t_information
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (information == null)
                {
                    throw new WebApiException(400, "头条不存在");
                }
                information.StartShowTime = request.StartShowTime;
                information.EndShowTime = request.EndShowTime;
                information.Content = request.Content;
                information.ContentIntroduction = request.ContentIntroduction;
                information.LastModified = DateTime.Now;
                information.Title = request.Title;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改头条状态
        /// </summary>
        public void ModifyInformationStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var information = (from item in db.t_information
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (information == null)
                {
                    throw new WebApiException(400, "头条不存在");
                }
                information.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除头条
        /// </summary>
        public void DeleteInformation(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var information = (from item in db.t_information
                                   where item.Id == request.Id
                                   select item).FirstOrDefault();
                if (information == null)
                {
                    throw new WebApiException(400, "头条不存在");
                }
                db.t_information.Remove(information);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询消息分组
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<MessageInfo> GetMessageList(PageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var messageGroup = from item in db.t_message_group
                                   select item;
                int totalCount = messageGroup.Count();

                return new PageRes<MessageInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in messageGroup
                            orderby item.CreateTime descending
                            select new MessageInfo
                            {
                                Status = item.Status,
                                CreateTime = item.CreateTime,
                                ImgUrl = item.ImgUrl,
                                Id = item.Id,
                                OrderIndex = item.OrderIndex,
                                Title = item.Title
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加消息分组
        /// </summary>
        /// <param name="request"></param>
        public void AddMessage(AddMessageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                db.t_message_group.Add(new t_message_group 
                {
                    Status=1,
                    CreateTime=DateTime.Now,
                    ImgUrl=request.ImgUrl,
                    LastModified=DateTime.Now,
                    OrderIndex=request.OrderIndex,
                    Title=request.Title
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑消息分组
        /// </summary>
        /// <param name="request"></param>
        public void ModifyMessage(ModifyMessageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var messageGroup = (from item in db.t_message_group
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (messageGroup == null)
                {
                    throw new WebApiException(400,"分组不存在");
                }

                messageGroup.ImgUrl = request.ImgUrl;
                messageGroup.LastModified = DateTime.Now;
                messageGroup.OrderIndex = request.OrderIndex;
                messageGroup.Title = request.Title;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改消息分组状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyMessageStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var messageGroup = (from item in db.t_message_group
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (messageGroup == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                messageGroup.Status = request.Status;
                messageGroup.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除消息分组
        /// </summary>
        /// <param name="request"></param>
        public void DeleteMessage(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var messageGroup = (from item in db.t_message_group
                                    where item.Id == request.Id
                                    select item).FirstOrDefault();
                if (messageGroup == null)
                {
                    throw new WebApiException(400, "分组不存在");
                }

                db.t_message_group.Remove(messageGroup);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询消息模板
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<MessageTemplateInfo> GetMessageTemplateList(GetMessageTemplateListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var template = from item in db.t_message_template
                               select item;
                if (request.Status != 0)
                {
                    template = from item in template
                               where item.Status == request.Status
                               select item;
                }

                int totalCount = template.Count();
                return new PageRes<MessageTemplateInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in template
                            join item2 in db.t_message_group on item.GroupId equals item2.Id into a from ai in a.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new MessageTemplateInfo
                            {
                                Status = item.Status,
                                Content = item.Content,
                                CreateTime = item.CreateTime,
                                GroupId = item.GroupId,
                                Id = item.Id,
                                GroupName = ai == null ? "" : ai.Title,
                                TempName = item.TempName,
                                Title = item.Title
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加消息模板
        /// </summary>
        /// <param name="request"></param>
        public void AddMessageTemplate(AddMessageTemplateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断分组是否存在
                var group = (from item in db.t_message_group
                             where item.Id == request.GroupId && item.Status==1
                             select item).FirstOrDefault();
                if (group == null)
                {
                    throw new WebApiException(400,"无效的消息分组");
                }

                db.t_message_template.Add(new t_message_template
                {
                    Content = request.Content,
                    Status = 1,
                    CreateTime = DateTime.Now,
                    GroupId = request.GroupId,
                    LastModified = DateTime.Now,
                    TempName = request.TempName,
                    Title = request.Title
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑消息模板
        /// </summary>
        /// <param name="request"></param>
        public void ModifyMessageTemplate(ModifyMessageTemplateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_message_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400,"模板不存在");
                }  
                //判断分组是否存在
                var group = (from item in db.t_message_group
                             where item.Id == request.GroupId && item.Status == 1
                             select item).FirstOrDefault();
                if (group == null)
                {
                    throw new WebApiException(400, "无效的消息分组");
                }
                template.Content = request.Content;
                template.GroupId = request.GroupId;
                template.LastModified = DateTime.Now;
                template.TempName = request.TempName;
                template.Title = request.Title;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改消息模板状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyMessageTemplateStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_message_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
               
                template.Status = request.Status;
                template.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除消息模板
        /// </summary>
        /// <param name="request"></param>
        public void DeleteMessageTemplate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var template = (from item in db.t_message_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                if (template == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }

                db.t_message_template.Remove(template);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询通知列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<NoticeInfo> GetNoticeList(PageRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var notice = from item in db.t_notice_setting
                             select item;

                int totalCount = notice.Count();
                var list = (from item in notice
                            join item4 in db.t_account_api on item.ApiUrl equals item4.Url into c
                            from ci in c.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new NoticeInfo
                            {
                                SendTimeRules = item.SendTimeRules,
                                PushTargetTag=item.PushTargetTag,
                                SendType = item.SendType,
                                SmsSignId = item.SmsSignId,
                                SmsOtherMobile = item.SmsOtherMobile,
                                Status = item.Status,
                                Decription = item.Decription,
                                ApiUrl = item.ApiUrl,
                                Id = item.Id,
                                CreateTime = item.CreateTime,
                                TimeType = item.TimeType,
                                SmsTarget = item.SmsTarget,
                                SmsTemplateId = item.SmsTemplateId,
                                ApiDes = ci == null ? "" : ci.Description,
                                SmsAppKey = item.SmsAppKey,
                                SmsChannelCode = item.SmsChannelCode,
                                SendOverSecond = item.SendOverSecond,
                                SmsTriggerAccount = item.SmsTriggerAccount
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                foreach (var item in list)
                {
                    if ((item.SendType & 1) > 0) 
                    {
                        item.SmsSignName = (from x in db.t_sms_sign
                                            where x.Id == item.SmsSignId
                                            select x.SignName).FirstOrDefault();
                        item.SmsChannelName= (from x in db.t_sms_channel
                                              where x.ChannelCode == item.SmsChannelCode
                                              select x.ChannelName).FirstOrDefault();
                        item.SmsAppName = (from x in db.t_sms_channel_app
                                           where x.ChannelCode == item.SmsChannelCode && x.AppKey==item.SmsAppKey
                                           select x.AppName).FirstOrDefault();
                        item.SmsTemplateName = (from x in db.t_sms_template
                                                where x.Id == item.SmsTemplateId
                                                select x.TempName).FirstOrDefault();
                    }
                    if ((item.SendType & 2) > 0) 
                    {
                        item.SmsChannelName = (from x in db.t_push_channel
                                               where x.ChannelCode == item.SmsChannelCode
                                               select x.ChannelName).FirstOrDefault();
                        item.SmsAppName = (from x in db.t_push_channel_app
                                           where x.ChannelCode == item.SmsChannelCode && x.AppKey == item.SmsAppKey
                                           select x.AppName).FirstOrDefault();
                        item.SmsTemplateName = (from x in db.t_push_template
                                                where x.Id == item.SmsTemplateId
                                                select x.TempName).FirstOrDefault();
                    }
                    if ((item.SendType & 4) > 0)
                    {
                        item.SmsTemplateName = (from x in db.t_message_template
                                                where x.Id == item.SmsTemplateId
                                                select x.TempName).FirstOrDefault();
                    }
                }

                return new PageRes<NoticeInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = list
                };
            }
        }

        /// <summary>
        /// 添加通知
        /// </summary>
        /// <param name="request"></param>
        public void AddNotice(addNoticeRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                if (request.SendType == 1)
                {
                    //判断短信签名是否有效
                    var sign = (from item in db.t_sms_sign
                                where item.Id == request.SmsSignId && item.ChannelCode== request.SmsChannelCode && item.AppKey==request.SmsAppKey
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new WebApiException(400,"无效的短信签名");
                    }
                    //判断模板是否有效
                    var temp = (from item in db.t_sms_template
                                where item.Id == request.SmsTemplateId && item.ChannelCode == request.SmsChannelCode && item.AppKey == request.SmsAppKey
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        throw new WebApiException(400,"无效的短信模板");
                    }
                }
                if (request.TimeType == 1)
                {
                    //判断api是否有效
                    var api = (from item in db.t_account_api
                               where item.Url == request.ApiUrl
                               select item).FirstOrDefault();
                    if (api == null)
                    {
                        throw new WebApiException(400,"无效的触发api");
                    }
                }

                db.t_notice_setting.Add(new t_notice_setting 
                { 
                    SendTimeRules=request.SendTimeRules,
                    SendType=request.SendType,
                    SmsSignId=request.SmsSignId,
                    SmsTarget=request.SmsTarget,
                    SmsOtherMobile=request.SmsOtherMobile,
                    SmsTemplateId=request.SmsTemplateId,
                    Status=1,
                    ApiUrl=request.ApiUrl,
                    CreateTime=DateTime.Now,
                    Decription=request.Decription,
                    LastModified=DateTime.Now,
                    PushTemplateId=request.PushTemplateId,
                    TimeType=request.TimeType,
                    SmsAppKey=request.SmsAppKey,
                    SmsChannelCode=request.SmsChannelCode,
                    SendOverSecond=request.SendOverSecond,
                    PushTargetTag=request.PushTargetTag,
                    SmsTriggerAccount =request.SmsTriggerAccount
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑通知
        /// </summary>
        /// <param name="request"></param>
        public void ModifyNotice(ModifyNoticeRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var notice = (from item in db.t_notice_setting
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (notice == null)
                {
                    throw new WebApiException(400,"数据不存在");
                }

                if (request.SendType == 1)
                {
                    //判断短信签名是否有效
                    var sign = (from item in db.t_sms_sign
                                where item.Id == request.SmsSignId && item.ChannelCode == request.SmsChannelCode && item.AppKey == request.SmsAppKey
                                select item).FirstOrDefault();
                    if (sign == null)
                    {
                        throw new WebApiException(400, "无效的短信签名");
                    }
                    //判断模板是否有效
                    var temp = (from item in db.t_sms_template
                                where item.Id == request.SmsTemplateId && item.ChannelCode == request.SmsChannelCode && item.AppKey == request.SmsAppKey
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        throw new WebApiException(400, "无效的短信模板");
                    }
                }
                if (request.TimeType == 1)
                {
                    //判断api是否有效
                    var api = (from item in db.t_account_api
                               where item.Url == request.ApiUrl
                               select item).FirstOrDefault();
                    if (api == null)
                    {
                        throw new WebApiException(400, "无效的触发api");
                    }
                }
                notice.SendTimeRules = request.SendTimeRules;
                notice.SendType = request.SendType;
                notice.SmsSignId = request.SmsSignId;
                notice.SmsTarget = request.SmsTarget;
                notice.SmsOtherMobile = request.SmsOtherMobile;
                notice.PushTargetTag = request.PushTargetTag;
                notice.SmsTemplateId = request.SmsTemplateId;
                notice.ApiUrl = request.ApiUrl;
                notice.Decription = request.Decription;
                notice.LastModified = DateTime.Now;
                notice.PushTemplateId = request.PushTemplateId;
                notice.TimeType = request.TimeType;
                notice.SmsChannelCode = request.SmsChannelCode;
                notice.SmsAppKey = request.SmsAppKey;
                notice.SendOverSecond = request.SendOverSecond;
                notice.SmsTriggerAccount = request.SmsTriggerAccount;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 修改通知状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyNoticeStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var notice = (from item in db.t_notice_setting
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (notice == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                notice.LastModified = DateTime.Now;
                notice.Status = request.Status;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除通知
        /// </summary>
        /// <param name="request"></param>
        public void DeleteNotice(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var notice = (from item in db.t_notice_setting
                              where item.Id == request.Id
                              select item).FirstOrDefault();
                if (notice == null)
                {
                    throw new WebApiException(400, "数据不存在");
                }
                db.t_notice_setting.Remove(notice);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 查询通知短信发送记录
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<NoticeSmsSendRecordInfo> GetNoticeSmsSendRecord(DetailsPageRequest request) 
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var record = from item in db.t_notice_send_record
                             where item.SettingId == request.Id && item.SendType == 1
                             select item;
                int totalCount = record.Count();

                return new PageRes<NoticeSmsSendRecordInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in record
                            join item2 in db.t_sms_channel on item.SmsChannelCode equals item2.ChannelCode into a from ai in a.DefaultIfEmpty()
                            join item3 in db.t_sms_channel_app on new { ChannelCode = item.SmsChannelCode, AppKey = item.SmsAppKey } equals new { item3.ChannelCode, item3.AppKey } into b from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new NoticeSmsSendRecordInfo
                            {
                                SendContent = item.SendContent,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                SmsTempPara = item.SmsTempPara,
                                SmsMobile = item.SmsMobile,
                                SmsAppName = bi == null ? "" : bi.AppName,
                                SmsChannelName = ai == null ? "" : ai.ChannelName,
                                ErrorMessage = item.IsSend == false ? (item.MaxSendTime < timeNow ? "发送超时" : "等待发送") : (item.ErrorMessage == null ? "已发送" : item.ErrorMessage)
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询通知短信发送详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<NoticeSmsSendDetailsInfo> GetNoticeSmsSendDetails(DetailsPageRequest request) 
        {
            DateTime timeNow = DateTime.Now;
            using (var db = new meal_ticketEntities())
            {
                var details = from item in db.t_notice_send_record_details
                             where item.RecordId == request.Id
                             select item;
                int totalCount = details.Count();

                int sendOverTime = 300;
                var sys = (from item in db.t_system_param
                           where item.ParamName == "SmsSendOverTime"
                           select item).FirstOrDefault();
                if (sys != null)
                {
                    sendOverTime = int.Parse(sys.ParamValue);
                }
                DateTime tempTime = timeNow.AddSeconds(-sendOverTime);
                return new PageRes<NoticeSmsSendDetailsInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in details
                            orderby item.CreateTime descending
                            select new NoticeSmsSendDetailsInfo
                            {
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                StatusDes=(item.Status==4000&& item.CreateTime< tempTime) ?"发送超时":item.StatusDes,
                                Mobile=item.Mobile,
                                ReceiveTime=item.ReceiveTime,
                                ThirdMsgId=item.ThirdMsgId
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 查询触发api列表
        /// </summary>
        /// <returns></returns>
        public List<NoticeApiInfo> GetNoticeApiList() 
        {
            using (var db = new meal_ticketEntities())
            {
                var api = (from item in db.t_account_api
                           orderby item.Description
                           select new NoticeApiInfo
                           {
                               ApiUrl = item.Url,
                               ApiDes = item.Description,
                               ApiPara=item.Para
                           }).ToList();
                return api;
            }
        }

        /// <summary>
        /// 查询短信模板列表
        /// </summary>
        /// <returns></returns>
        public PageRes<SmsTempInfo> GetSmsTemplateList(GetSmsTemplateListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = from item in db.t_sms_template
                           select item;
                if (!string.IsNullOrEmpty(request.ChannelCode))
                {
                    temp = from item in temp
                           where item.ChannelCode == request.ChannelCode
                           select item;
                }
                if (!string.IsNullOrEmpty(request.AppKey))
                {
                    temp = from item in temp
                           where item.AppKey == request.AppKey
                           select item;
                }
                if (request.Status != 0)
                {
                    temp = from item in temp
                           where item.Status == request.Status
                           select item;
                }
                if (request.ExamineStatus != 0)
                {
                    temp = from item in temp
                           where item.ExamineStatus == request.ExamineStatus
                           select item;
                }
                int totalCount = temp.Count();

                return new PageRes<SmsTempInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in temp
                            join item2 in db.t_sms_channel on item.ChannelCode equals item2.ChannelCode into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_sms_channel_app on new { item.ChannelCode, item.AppKey } equals new { item3.ChannelCode, item3.AppKey } into b
                            from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new SmsTempInfo
                            {
                                Status = item.Status,
                                ExamineStatus = item.ExamineStatus,
                                AppKey = item.AppKey,
                                ApplyTime = item.ApplyTime,
                                AppName = bi == null ? "" : bi.AppName,
                                ChannelCode = item.ChannelCode,
                                ChannelName = ai == null ? "" : ai.ChannelName,
                                CreateTime = item.CreateTime,
                                ExamineFailReason = item.ExamineFailReason,
                                ExamineTime = item.ExamineTime,
                                Id = item.Id,
                                TempContent = item.TempContent,
                                TempName = item.TempName,
                                TempType = item.TempType,
                                ThirdTempId = item.ThirdTempId
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加短信模板
        /// </summary>
        public void AddSmsTemplate(AddSmsTemplateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断渠道是否存在
                var channel = (from item in db.t_sms_channel
                               where item.ChannelCode == request.ChannelCode && item.Status == 1
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "无效的渠道");
                }
                //判断应用是否存在
                var app = (from item in db.t_sms_channel_app
                           where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "无效的应用");
                }

                db.t_sms_template.Add(new t_sms_template
                {
                    TempContent = request.TempContent,
                    Status = 1,
                    ExamineStatus = 1,
                    ThirdTempId = "",
                    AppKey = request.AppKey,
                    ChannelCode = request.ChannelCode,
                    CreateTime = DateTime.Now,
                    ExamineFailReason = "",
                    LastModified = DateTime.Now,
                    TempLink = "",
                    TempName=request.TempName,
                    TempType=request.TempType,
                    ApplyTime = null,
                    ExamineTime = null
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑短信模板
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsTemplate(ModifySmsTemplateRequest request)
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var temp = (from item in db.t_sms_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    if (temp.ExamineStatus != 1 && temp.ExamineStatus != 4)
                    {
                        throw new WebApiException(400, "当前审核状态无法修改");
                    }
                    //判断渠道是否存在
                    var channel = (from item in db.t_sms_channel
                                   where item.ChannelCode == request.ChannelCode && item.Status == 1
                                   select item).FirstOrDefault();
                    if (channel == null)
                    {
                        throw new WebApiException(400, "无效的渠道");
                    }
                    //判断应用是否存在
                    var app = (from item in db.t_sms_channel_app
                               where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                               select item).FirstOrDefault();
                    if (app == null)
                    {
                        throw new WebApiException(400, "无效的应用");
                    }
                    string sourceChannelCode = temp.ChannelCode;
                    string sourceAppKey = temp.AppKey;
                    string sourceAppSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == sourceChannelCode && item.AppKey == sourceAppKey
                                   select item).FirstOrDefault();
                    if (tempApp != null)
                    {
                        sourceAppSecret = tempApp.AppSecret;
                    }
                    string sourceThirdTempId = temp.ThirdTempId;

                    temp.AppKey = request.AppKey;
                    temp.ChannelCode = request.ChannelCode;
                    temp.ExamineFailReason = "";
                    temp.ExamineStatus = 1;
                    temp.LastModified = DateTime.Now;
                    temp.TempContent = request.TempContent;
                    temp.TempName = request.TempName;
                    temp.TempType = request.TempType;

                    if (sourceChannelCode == request.ChannelCode && sourceAppKey == request.AppKey)
                    {
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(sourceThirdTempId) && !string.IsNullOrEmpty(sourceAppSecret))
                        {
                            //删除原渠道签名
                            ThirdSmsBase smsObj = new ThirdSmsBase(sourceChannelCode, sourceAppKey, sourceAppSecret);
                            var thirdSms = smsObj.GetThirdSmsObj();
                            thirdSms.DeleteSmsTemplete(sourceThirdTempId);
                        }
                        temp.ThirdTempId = "";
                    }
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 修改短信模板状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifySmsTemplateStatus(ModifyStatusRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_sms_template
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                temp.Status = request.Status;
                temp.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除短信模板
        /// </summary>
        /// <param name="request"></param>
        public void DeleteSmsTemplate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_sms_template
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                db.t_sms_template.Remove(temp);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(temp.ThirdTempId))
                {
                    string appSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == temp.ChannelCode && item.AppKey == temp.AppKey
                                   select item).FirstOrDefault();
                    if (tempApp != null)
                    {
                        appSecret = tempApp.AppSecret;
                    }
                    if (!string.IsNullOrEmpty(appSecret))
                    {
                        //删除原渠道模板
                        ThirdSmsBase smsObj = new ThirdSmsBase(temp.ChannelCode, temp.AppKey, appSecret);
                        var thirdSms = smsObj.GetThirdSmsObj();
                        thirdSms.DeleteSmsTemplete(temp.ThirdTempId);
                    }
                }
            }
        }

        /// <summary>
        /// 短信模板提交审核
        /// </summary>
        /// <param name="request"></param>
        public void SmsTempApplyExamine(DetailsRequest request) {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var temp = (from item in db.t_sms_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        throw new WebApiException(400, "签名不存在");
                    }
                    if (temp.ExamineStatus != 1)
                    {
                        throw new WebApiException(400, "当前状态无法提交审核");
                    }
                    temp.ExamineStatus = 2;
                    temp.ApplyTime = DateTime.Now;
                    db.SaveChanges();

                    string channelCode = temp.ChannelCode;
                    string appKey = temp.AppKey;
                    string appSecret = string.Empty;
                    var tempApp = (from item in db.t_sms_channel_app
                                   where item.ChannelCode == channelCode && item.AppKey == appKey
                                   select item).FirstOrDefault();
                    if (tempApp == null)
                    {
                        throw new WebApiException(400, "无效的应用");
                    }
                    appSecret = tempApp.AppSecret;

                    ThirdSmsBase smsObj = new ThirdSmsBase(channelCode, appKey, appSecret);
                    var thirdSms = smsObj.GetThirdSmsObj();
                    HttpResponse res;
                    if (string.IsNullOrEmpty(temp.ThirdTempId))
                    {
                        res = thirdSms.CreateSmsTemplate(new SmsTemplateInfo
                        {
                            Content = temp.TempContent,
                            Type = temp.TempType
                        });
                    }
                    else
                    {
                        res = thirdSms.UpdateSmsTemplate(new SmsTemplateInfo
                        {
                            TemplateId=temp.ThirdTempId,
                            Content = temp.TempContent,
                            Type = temp.TempType
                        });
                    }
                    var resObj = JsonConvert.DeserializeObject<dynamic>(res.Content);
                    if (resObj.error != null)
                    {
                        string errorMessage = resObj.error.code;
                        errorMessage = errorMessage.JGErrorParse();
                        throw new WebApiException(400, errorMessage);
                    }
                    temp.ThirdTempId = resObj.temp_id;
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 查询推送模板列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PageRes<PushTemplateInfo> GetPushTemplateList(GetPushTemplateListRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                var template = from item in db.t_push_template
                               select item;
                if (request.Status != 0)
                {
                    template = from item in template
                               where item.Status == request.Status
                               select item;
                }

                int totalCount = template.Count();
                return new PageRes<PushTemplateInfo>
                {
                    MaxId = 0,
                    TotalCount = totalCount,
                    List = (from item in template
                            join item2 in db.t_push_channel on item.ChannelCode equals item2.ChannelCode into a
                            from ai in a.DefaultIfEmpty()
                            join item3 in db.t_push_channel_app on new { item.ChannelCode, item.AppKey } equals new { item3.ChannelCode, item3.AppKey } into b
                            from bi in b.DefaultIfEmpty()
                            orderby item.CreateTime descending
                            select new PushTemplateInfo
                            {
                                Status = item.Status,
                                AppKey = item.AppKey,
                                ChannelCode = item.ChannelCode,
                                JumpUrl=item.JumpUrl,
                                CreateTime = item.CreateTime,
                                Id = item.Id,
                                TempContent = item.TempContent,
                                TempName = item.TempName,
                                AppName = bi == null ? "" : bi.AppName,
                                ChannelName = ai == null ? "" : ai.ChannelName
                            }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList()
                };
            }
        }

        /// <summary>
        /// 添加推送模板
        /// </summary>
        /// <param name="request"></param>
        public void AddPushTemplate(AddPushTemplateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            {
                //判断渠道是否存在
                var channel = (from item in db.t_push_channel
                               where item.ChannelCode == request.ChannelCode && item.Status == 1
                               select item).FirstOrDefault();
                if (channel == null)
                {
                    throw new WebApiException(400, "无效的渠道");
                }
                //判断应用是否存在
                var app = (from item in db.t_push_channel_app
                           where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                           select item).FirstOrDefault();
                if (app == null)
                {
                    throw new WebApiException(400, "无效的应用");
                }

                db.t_push_template.Add(new t_push_template
                {
                    TempContent = request.TempContent,
                    Status = 1,
                    AppKey = request.AppKey,
                    ChannelCode = request.ChannelCode,
                    CreateTime = DateTime.Now,
                    LastModified = DateTime.Now,
                    TempName = request.TempName,
                    JumpUrl=request.JumpUrl
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 编辑推送模板
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushTemplate(ModifyPushTemplateRequest request) 
        {
            using (var db = new meal_ticketEntities())
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var temp = (from item in db.t_push_template
                                where item.Id == request.Id
                                select item).FirstOrDefault();
                    if (temp == null)
                    {
                        throw new WebApiException(400, "模板不存在");
                    }
                    //判断渠道是否存在
                    var channel = (from item in db.t_push_channel
                                   where item.ChannelCode == request.ChannelCode && item.Status == 1
                                   select item).FirstOrDefault();
                    if (channel == null)
                    {
                        throw new WebApiException(400, "无效的渠道");
                    }
                    //判断应用是否存在
                    var app = (from item in db.t_push_channel_app
                               where item.ChannelCode == request.ChannelCode && item.AppKey == request.AppKey && item.Status == 1
                               select item).FirstOrDefault();
                    if (app == null)
                    {
                        throw new WebApiException(400, "无效的应用");
                    }

                    temp.AppKey = request.AppKey;
                    temp.ChannelCode = request.ChannelCode;
                    temp.LastModified = DateTime.Now;
                    temp.TempContent = request.TempContent;
                    temp.TempName = request.TempName;
                    temp.JumpUrl = request.JumpUrl;
                    db.SaveChanges();

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 修改推送模板状态
        /// </summary>
        /// <param name="request"></param>
        public void ModifyPushTemplateStatus(ModifyStatusRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_push_template
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                temp.Status = request.Status;
                temp.LastModified = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 删除推送模板
        /// </summary>
        /// <param name="request"></param>
        public void DeletePushTemplate(DeleteRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_push_template
                            where item.Id == request.Id
                            select item).FirstOrDefault();
                if (temp == null)
                {
                    throw new WebApiException(400, "模板不存在");
                }
                db.t_push_template.Remove(temp);
                db.SaveChanges();
            }
        }
        #endregion
    }
}
