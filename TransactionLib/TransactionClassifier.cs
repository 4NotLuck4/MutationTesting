namespace TransactionLib
{
    public enum TransactionKind
    {
        Unknown,
        Deposit,
        Withdrawal,
        Transfer
    }

    public class Transaction
    {
        public decimal Amount { get; set; }
        public TransactionKind Kind { get; set; }
        public bool IsInternal { get; set; }
        public DateTime Timestamp { get; set; }
        public string FromAccountType { get; set; }
        public string ToAccountType { get; set; }
        public bool IsVipClient { get; set; }
        public string Channel { get; set; } // "Mobile", "Web", "ATM", "Office"
        public int DailyTransactionCount { get; set; }
        public decimal DailyTransactionTotal { get; set; }
    }

    public static class TransactionAnalyzer
    {
        /// <summary>
        /// Анализирует банковскую транзакцию по множеству условий, таких как сумма,
        /// тип транзакции, лимиты клиента, канал проведения и др.
        /// </summary>
        /// <param name="tx">Объект транзакции с подробной информацией.</param>
        /// <returns>
        /// Строка, описывающая результат анализа:
        /// - "Транзакция допустима." — если транзакция удовлетворяет всем условиям;
        /// - сообщение об ошибке или ограничении — в противном случае.
        /// </returns>
        public static string AnalyzeTransaction(Transaction tx)
        {
            // Проверка на корректность суммы.
            if (tx.Amount <= 0)
                return "Ошибка: сумма должна быть положительной.";

            // Проверка, что указан допустимый тип транзакции.
            if (tx.Kind == TransactionKind.Unknown)
                return "Ошибка: неизвестный тип транзакции.";

            // Ограничение на количество транзакций в день для обычных клиентов.
            if (tx.DailyTransactionCount > 10 && !tx.IsVipClient)
                return "Ограничение: превышено количество операций в день.";

            // Проверка суточного лимита по сумме транзакций для non-VIP.
            if (tx.DailyTransactionTotal + tx.Amount > 500_000 && !tx.IsVipClient)
                return "Ограничение: превышена дневная сумма транзакций.";

            // Банкоматы не поддерживают крупные транзакции.
            if (tx.Channel == "ATM" && tx.Amount > 100_000)
                return "Ограничение: банкоматы не обрабатывают крупные суммы.";

            // Дополнительные проверки для переводов между счетами.
            if (tx.Kind == TransactionKind.Transfer)
            {
                // Ограничение на внешние переводы большого объема для non-VIP.
                if (!tx.IsInternal && tx.Amount > 1_000_000 && !tx.IsVipClient)
                    return "Отклонено: крупные внешние переводы недопустимы.";

                // Комиссия за межтиповой перевод (например, с текущего на сберегательный)
                if (tx.FromAccountType != tx.ToAccountType && !tx.IsInternal)
                {
                    if (!tx.IsVipClient)
                        return "Комиссия: 1% за перевод между типами счетов.";
                }

                // Онлайн-переводы между разными типами счетов запрещены в выходные.
                if (IsWeekend(tx.Timestamp) && tx.FromAccountType != tx.ToAccountType)
                {
                    if (tx.Channel == "Web" || tx.Channel == "Mobile")
                        return "Ошибка: в выходные нельзя переводить между разными типами счетов через онлайн-каналы.";
                }
            }

            // Правила, специфичные для снятия наличных.
            if (tx.Kind == TransactionKind.Withdrawal)
            {
                // Запрет на снятие через веб-банкинг.
                if (tx.Channel == "Web")
                    return "Отклонено: снятие через веб-банкинг запрещено.";

                // Ограничение на сумму снятия для внешних операций у non-VIP.
                if (!tx.IsInternal && tx.Amount > 200_000 && !tx.IsVipClient)
                    return "Ограничение: крупное снятие только для VIP-клиентов.";
            }

            // Если все проверки пройдены — транзакция допустима.
            return "Транзакция допустима.";
        }

        /// <summary>
        /// Проверяет, является ли указанная дата выходным днём (суббота или воскресенье).
        /// </summary>
        /// <param name="time">Дата, которую необходимо проверить.</param>
        /// <returns>True, если день — суббота или воскресенье.</returns>
        private static bool IsWeekend(DateTime time)
        {
            return time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday;
        }
    }

}
