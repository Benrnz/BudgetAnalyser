﻿using BudgetAnalyser.Engine.Mobile;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger.Data.Data2;

public class MapperMobileSettingsToDto2 : IDtoMapper<MobileStorageSettingsDto, MobileStorageSettings>
{
    public MobileStorageSettingsDto ToDto(MobileStorageSettings model)
    {
        var dto = new MobileStorageSettingsDto { AccessKeyId = model.AccessKeyId, AccessKeySecret = model.AccessKeySecret, AmazonS3Region = model.AmazonS3Region };
        return dto;
    }

    public MobileStorageSettings ToModel(MobileStorageSettingsDto dto)
    {
        var model = new MobileStorageSettings { AccessKeyId = dto.AccessKeyId, AccessKeySecret = dto.AccessKeySecret, AmazonS3Region = dto.AmazonS3Region };
        return model;
    }
}
