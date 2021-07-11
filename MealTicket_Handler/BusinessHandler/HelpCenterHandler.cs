using MealTicket_DBCommon;
using MealTicket_Handler.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealTicket_Handler
{
    public class HelpCenterHandler
    {
        public List<QuestionType> GetQuestionType()
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_helpcenter_question_type
                            orderby item.OrderIndex
                            select new QuestionType
                            {
                                TypeIcon = item.IconUrl,
                                TypeId = item.Id,
                                TypeName = item.TypeName
                            }).ToList();
                return temp;
            }
        }

        public PageRes<Question> GetQuestionList(GetQuestionListRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var question = from item in db.t_helpcenter_question_content
                               where item.Status == 1
                               select item;
                var type = from item in db.t_helpcenter_question_type
                           join item2 in db.t_helpcenter_question_type_content_rel.AsNoTracking() on item.Id equals item2.TypeId
                           select new{item,item2};


                var temp = from item in question
                           join item2 in type on item.Id equals item2.item2.ContentId into a
                           from ai in a.DefaultIfEmpty()
                           select new { item, ai };
                if (request.TypeId != 0)
                {
                    temp = from item in temp
                           where item.ai != null && item.ai.item.Id == request.TypeId
                           select item;
                }
                if (!string.IsNullOrEmpty(request.Key))
                {
                    temp = from item in temp
                           where item.item.Name.Contains(request.Key)
                           select item;
                }
                if (request.IsCommon)
                {
                    temp = from item in temp
                           where item.item.IsCommon == true
                           select item;
                }
                if (request.MaxId > 0)
                {
                    temp = from item in temp
                           where item.item.Id <= request.MaxId
                           select item;
                }

                temp = from item in temp
                       group item by item.item.Id into g
                       select g.FirstOrDefault();
                
                int totalCount = temp.Count();
                long maxId = 0;
                if (totalCount > 0)
                {
                    maxId = temp.Max(e => e.item.Id);
                }

                var result = (from item in temp
                              orderby item.item.IsTop descending, item.item.OrderIndex
                              select new Question
                              {
                                  QuestionId = item.item.Id,
                                  QuestName = item.item.Name,
                                  QuestionTypeName = item.ai == null ? "" : item.ai.item.TypeName,
                                  UpdateTime=item.item.UpdateTime,
                                  IsTop=item.item.IsTop
                              }).Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                return new PageRes<Question>
                {
                    TotalCount=totalCount,
                    MaxId=maxId,
                    List=result
                };
            }
        }

        public QuestionDetails GetQuestionDetails(DetailsRequest request)
        {
            using (var db = new meal_ticketEntities())
            {
                var temp = (from item in db.t_helpcenter_question_content
                            where item.Id == request.Id
                            select new QuestionDetails
                            {
                                QuestionId = item.Id,
                                UpdateTime = item.UpdateTime,
                                QuestName = item.Name,
                                Answer = item.Answer,
                                IsTop=item.IsTop
                            }).FirstOrDefault();
                return temp;
            }
        }
    }
}
