namespace MacroCreator.Curve;

/// <summary>
/// 提供模拟人类鼠标移动的方法
/// </summary>
public static class HumanMouseMover
{
    // 随机数生成器，用于抖动和路径弯曲
    private static readonly Random _random = new();

    // 抖动的最大幅度（像素）
    private const double JITTER_STRENGTH = 2.5;

    // 路径弯曲的最大程度（占总距离的百分比）
    private const double CURVE_STRENGTH_FACTOR = 0.3;

    /// <summary>
    /// 生成一个模拟人类鼠标移动的步骤列表
    /// </summary>
    /// <param name="totalDx">总的X轴偏移量</param>
    /// <param name="totalDy">总的Y轴偏移量</param>
    /// <param name="totalMilliseconds">希望的总移动时间（毫秒）</param>
    /// <param name="minStepDelay">每一步的最小延迟（毫秒）。模拟鼠标的“刷新率”。</param>
    /// <returns>一个包含所有移动步骤的列表</returns>
    public static List<MouseStep> GenerateHumanMovement(int totalDx, int totalDy, int totalMilliseconds, int minStepDelay = 10)
    {
        var steps = new List<MouseStep>();

        // 1. 定义贝塞尔曲线的起点、终点和控制点
        var startPoint = new { X = 0.0, Y = 0.0 };
        var endPoint = new { X = (double)totalDx, Y = (double)totalDy };

        // 随机生成一个控制点，使路径弯曲
        // 控制点大致在起点和终点的中点附近，但有一定的随机偏移
        double totalDistance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        double deviation = totalDistance * CURVE_STRENGTH_FACTOR;

        var controlPoint = new
        {
            X = totalDx * 0.5 + (_random.NextDouble() - 0.5) * deviation,
            Y = totalDy * 0.5 + (_random.NextDouble() - 0.5) * deviation
        };

        // 2. 循环生成步骤，直到总时间耗尽
        double totalTimeElapsed = 0;
        double lastX = 0;
        double lastY = 0;

        while (totalTimeElapsed < totalMilliseconds)
        {
            // 3. 计算当前步骤的延迟时间，加入轻微随机性
            int currentDelay = Math.Max(minStepDelay, minStepDelay + _random.Next(-4, 5));

            // 确保最后一步不会超过总时间
            currentDelay = (int)Math.Min(currentDelay, totalMilliseconds - totalTimeElapsed);
            if (currentDelay <= 0) break;

            totalTimeElapsed += currentDelay;

            // 4. 计算时间进度 (t_time, 0.0 到 1.0)
            double timeProgress = totalTimeElapsed / totalMilliseconds;

            // 5. 应用缓动函数 (Ease-In-Out) 来获取“路径进度”
            // 这使得移动在开始时慢，中间快，结束时又慢
            double pathProgress = EaseInOutCubic(timeProgress);

            // 6. 根据“路径进度” (pathProgress)，在贝塞尔曲线上找到目标点
            double targetX = GetQuadraticBezierPoint(pathProgress, startPoint.X, controlPoint.X, endPoint.X);
            double targetY = GetQuadraticBezierPoint(pathProgress, startPoint.Y, controlPoint.Y, endPoint.Y);

            // 7. 添加随机抖动 (Jitter)
            double jitterX = (_random.NextDouble() - 0.5) * JITTER_STRENGTH * (1.0 - pathProgress); // 抖动在快结束时减小
            double jitterY = (_random.NextDouble() - 0.5) * JITTER_STRENGTH * (1.0 - pathProgress);

            double currentX = targetX + jitterX;
            double currentY = targetY + jitterY;

            // 8. 计算与上一步的 *增量* (Delta)
            int deltaX = (int)Math.Round(currentX - lastX);
            int deltaY = (int)Math.Round(currentY - lastY);

            // 9. 存储这一步
            if (deltaX != 0 || deltaY != 0)
            {
                steps.Add(new MouseStep { DeltaX = deltaX, DeltaY = deltaY, DelayMs = currentDelay });
            }

            // 10. 更新“上一步”的位置
            lastX = currentX;
            lastY = currentY;
        }

        // 11. 修正步骤：由于舍入和抖动，总和可能不完全等于目标
        // 添加最后一步来精确匹配总偏移量
        int totalMovedX = steps.Sum(s => s.DeltaX);
        int totalMovedY = steps.Sum(s => s.DeltaY);

        int finalDeltaX = totalDx - totalMovedX;
        int finalDeltaY = totalDy - totalMovedY;

        if (finalDeltaX != 0 || finalDeltaY != 0)
        {
            steps.Add(new MouseStep { DeltaX = finalDeltaX, DeltaY = finalDeltaY, DelayMs = minStepDelay });
        }

        return steps;
    }

    /// <summary> 
    /// 生成一个模拟人类鼠标移动的步骤列表
    /// </summary>
    /// <param name="totalDx">总的X轴偏移量</param>
    /// <param name="totalDy">总的Y轴偏移量</param>
    /// <param name="totalMilliseconds">希望的总移动时间（毫秒）</param>
    /// <param name="minStepDelay">每一步的最小延迟（毫秒）。模拟鼠标的“刷新率”。</param>
    /// <returns>一个包含所有移动步骤的异步可枚举序列</returns>
    public static async IAsyncEnumerable<MouseStep> GenerateHumanMovementAsync(int totalDx, int totalDy, int totalMilliseconds, int minStepDelay = 10)
    {
        // 4. 不再需要 List<MouseStep>

        // 1. 定义贝塞尔曲线的起点、终点和控制点
        var startPoint = new { X = 0.0, Y = 0.0 };
        var endPoint = new { X = (double)totalDx, Y = (double)totalDy };

        // 随机生成一个控制点，使路径弯曲
        // 控制点大致在起点和终点的中点附近，但有一定的随机偏移
        double totalDistance = Math.Sqrt(totalDx * totalDx + totalDy * totalDy);
        double deviation = totalDistance * CURVE_STRENGTH_FACTOR;

        var controlPoint = new
        {
            X = totalDx * 0.5 + (_random.NextDouble() - 0.5) * deviation,
            Y = totalDy * 0.5 + (_random.NextDouble() - 0.5) * deviation
        };

        // 2. 循环生成步骤，直到总时间耗尽
        double totalTimeElapsed = 0;
        double lastX = 0;
        double lastY = 0;

        // 5. 添加变量来跟踪已移动的总量（用于修正）
        int totalMovedX = 0;
        int totalMovedY = 0;

        while (totalTimeElapsed < totalMilliseconds)
        {
            // 3. 计算当前步骤的延迟时间，加入轻微随机性
            int currentDelay = Math.Max(minStepDelay, minStepDelay + _random.Next(-4, 5));

            // 确保最后一步不会超过总时间
            currentDelay = (int)Math.Min(currentDelay, totalMilliseconds - totalTimeElapsed);
            if (currentDelay <= 0) break;

            totalTimeElapsed += currentDelay;

            // 4. 计算时间进度 (t_time, 0.0 到 1.0)
            double timeProgress = totalTimeElapsed / totalMilliseconds;

            // 5. 应用缓动函数 (Ease-In-Out) 来获取“路径进度”
            // 这使得移动在开始时慢，中间快，结束时又慢
            double pathProgress = EaseInOutCubic(timeProgress);

            // 6. 根据“路径进度” (pathProgress)，在贝塞尔曲线上找到目标点
            double targetX = GetQuadraticBezierPoint(pathProgress, startPoint.X, controlPoint.X, endPoint.X);
            double targetY = GetQuadraticBezierPoint(pathProgress, startPoint.Y, controlPoint.Y, endPoint.Y);

            // 7. 添加随机抖动 (Jitter)
            double jitterX = (_random.NextDouble() - 0.5) * JITTER_STRENGTH * (1.0 - pathProgress); // 抖动在快结束时减小
            double jitterY = (_random.NextDouble() - 0.5) * JITTER_STRENGTH * (1.0 - pathProgress);

            double currentX = targetX + jitterX;
            double currentY = targetY + jitterY;

            // 8. 计算与上一步的 *增量* (Delta)
            int deltaX = (int)Math.Round(currentX - lastX);
            int deltaY = (int)Math.Round(currentY - lastY);

            // 9. 存储这一步
            if (deltaX != 0 || deltaY != 0)
            {
                // 6. 使用 yield return 返回步骤
                yield return new MouseStep { DeltaX = deltaX, DeltaY = deltaY };

                // 7. 在返回后，异步等待
                await Task.Delay(currentDelay);

                // 8. 跟踪总和
                totalMovedX += deltaX;
                totalMovedY += deltaY;
            }
            else
            {
                // 即使没有移动，也要等待，以消耗总时间
                await Task.Delay(currentDelay);
            }


            // 10. 更新“上一步”的位置
            lastX = currentX;
            lastY = currentY;
        }

        // 11. 修正步骤：由于舍入和抖动，总和可能不完全等于目标
        // 9. 使用 totalMovedX/Y 进行修正
        int finalDeltaX = totalDx - totalMovedX;
        int finalDeltaY = totalDy - totalMovedY;

        if (finalDeltaX != 0 || finalDeltaY != 0)
        {
            // 10. yield return 最后一个修正步骤
            yield return new MouseStep { DeltaX = finalDeltaX, DeltaY = finalDeltaY };
            await Task.Delay(minStepDelay);
        }

        // 11. 不需要 return steps;
    }

    /// <summary>
    /// 立方缓动函数 (Ease-In-Out)
    /// t: 0.0 到 1.0
    /// </summary>
    private static double EaseInOutCubic(double t)
    {
        if (t < 0.5)
        {
            return 4 * t * t * t;
        }
        else
        {
            double f = 2 * t - 2;
            return 0.5 * f * f * f + 1;
        }
    }

    /// <summary>
    /// 计算二次贝塞尔曲线上的点
    /// t: 0.0 到 1.0
    /// p0: 起点, p1: 控制点, p2: 终点
    /// </summary>
    private static double GetQuadraticBezierPoint(double t, double p0, double p1, double p2)
    {
        double oneMinusT = 1.0 - t;
        return oneMinusT * oneMinusT * p0 + 2.0 * oneMinusT * t * p1 + t * t * p2;
    }
}
