SELECT TOP (1000) [id]
      ,[name]
      ,[description]
      ,[image_url]
      ,[is_active]
      ,[created_date]
      ,[is_deleted]
  FROM [dbo].[simulation_components]


  INSERT INTO [dbo].[simulation_components]
    ([name], [description], [image_url], [is_active], [created_date], [is_deleted])
VALUES
-- 1. Cabin
(N'Cabin', N'The driver cabin of the Hyundai Mighty EX8. Contains seating, main dashboard, and access to truck controls.', NULL, 1, GETDATE(), 0),

-- 2. Main Dashboard
(N'Main Dashboard', N'Dashboard with speedometer, tachometer, warning lights, and main vehicle controls.', NULL, 1, GETDATE(), 0),

-- 3. Steering Wheel
(N'Steering Wheel', N'Steering wheel for maneuvering the truck during driving and positioning for lifting.', NULL, 1, GETDATE(), 0),

-- 4. Accelerator Pedal
(N'Accelerator Pedal', N'Pedal used to control the speed of the truck.', NULL, 1, GETDATE(), 0),

-- 5. Brake Pedal
(N'Brake Pedal', N'Pedal used to slow down or stop the vehicle.', NULL, 1, GETDATE(), 0),

-- 6. Gear Lever
(N'Gear Lever', N'Gear shifter for changing truck driving modes.', NULL, 1, GETDATE(), 0),

-- 7. PTO Control
(N'PTO Control', N'Power Take-Off switch to engage/disengage crane operation.', NULL, 1, GETDATE(), 0),

-- 8. Outriggers
(N'Outriggers', N'Supporting legs to stabilize the truck before operating the crane.', NULL, 1, GETDATE(), 0),

-- 9. Crane Base (Unic 340)
(N'Crane Base (Unic 340)', N'Base of the Unic 340 crane attached to the Hyundai Mighty EX8 chassis.', NULL, 1, GETDATE(), 0),

-- 10. Crane Boom (Unic 340)
(N'Crane Boom (Unic 340)', N'Telescopic boom arm of the Unic 340 used for lifting.', NULL, 1, GETDATE(), 0),

-- 11. Crane Hook
(N'Crane Hook', N'Metal hook attached to the boom for lifting loads.', NULL, 1, GETDATE(), 0),

-- 12. Crane Control Levers
(N'Crane Control Levers', N'Manual levers for operating the Unic 340 crane movements (lifting, lowering, extending, rotating).', NULL, 1, GETDATE(), 0),

-- 13. Safety Devices
(N'Safety Devices', N'Includes load moment limiter, warning buzzer, and other safety features on the Unic 340.', NULL, 1, GETDATE(), 0);
