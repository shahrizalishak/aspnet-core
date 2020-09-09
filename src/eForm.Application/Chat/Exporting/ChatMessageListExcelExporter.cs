﻿using System.Collections.Generic;
using System.Linq;
using Abp;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using eForm.Chat.Dto;
using eForm.DataExporting.Excel.EpPlus;
using eForm.Dto;
using eForm.Storage;

namespace eForm.Chat.Exporting
{
    public class ChatMessageListExcelExporter : EpPlusExcelExporterBase, IChatMessageListExcelExporter
    {
        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public ChatMessageListExcelExporter(
            ITempFileCacheManager tempFileCacheManager,
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession
            ) : base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages)
        {
            var tenancyName = messages.Count > 0 ? messages.First().TargetTenantName : L("Anonymous");
            var userName = messages.Count > 0 ? messages.First().TargetUserName : L("Anonymous");

            return CreateExcelPackage(
                $"Chat_{tenancyName}_{userName}.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Messages"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("ChatMessage_From"),
                        L("ChatMessage_To"),
                        L("Message"),
                        L("ReadState"),
                        L("CreationTime")
                    );

                    AddObjects(
                        sheet, 2, messages,
                        _ => _.Side == ChatSide.Receiver ? (_.TargetTenantName + "/" + _.TargetUserName) : L("You"),
                        _ => _.Side == ChatSide.Receiver ? L("You") : (_.TargetTenantName + "/" + _.TargetUserName),
                        _ => _.Message,
                        _ => _.Side == ChatSide.Receiver ? _.ReadState : _.ReceiverReadState,
                        _ => _timeZoneConverter.Convert(_.CreationTime, user.TenantId, user.UserId)
                    );

                    //Formatting cells
                    var timeColumn = sheet.Column(5);
                    timeColumn.Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                });
        }
    }
}