﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Domain.Entities;

public class OutboxEvent
{
    public string EventName { get; set; }
}
