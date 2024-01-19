﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using INVENTARIO.Entity;
using INVENTARIO.Services;

namespace INVENTARIO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly cifrado _cifrado;
        private readonly string _defaultConnection = "server=localhost;database=inventory;User ID=marcos;Password=marcos123;";

        public OrderController(cifrado cifrado)
        {
            _cifrado = cifrado ?? throw new ArgumentNullException(nameof(cifrado));
        }

        private async Task<User> ValidateTokenAndGetUser(string token, SampleContext context)
        {
            var vtoken = _cifrado.validarToken(token);

            if (vtoken == null)
            {
                throw new UnauthorizedAccessException("The token isn't valid!");
            }

            return await context.User
                .FirstOrDefaultAsync(res => res.Email.Equals(vtoken[1]) && res.Password.Equals(vtoken[2]));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var orderList = await context.Order.ToListAsync();

                    return Ok(orderList);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<Order>> GetOrderById(int orderId, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var order = await context.Order.FindAsync(orderId);

                    if (order == null)
                    {
                        return NotFound("No order found");
                    }
                    return Ok(order);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("orderdate/{orderDate}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderByOrderDate(DateTime orderDate, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var orderList = await context.Order
                        .Where(order => order.OrderDate == orderDate)
                        .ToListAsync();

                    return Ok(orderList);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }

        // Other date-specific methods (reception, dispatched, delivery) follow the same pattern

        [HttpGet("range/{startDate}/{endDate}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDateRange(DateTime startDate, DateTime endDate, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var ordersInRange = await context.Order
                        .Where(order => order.OrderDate.Date >= startDate.Date && order.OrderDate.Date <= endDate.Date)
                        .ToListAsync();

                    return Ok(ordersInRange);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update")]
        public async Task<ActionResult> PutOrder(Order order, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var existingOrder = await context.Order.FirstOrDefaultAsync(res => res.OrderId.Equals(order.OrderId));
                    if (existingOrder == null)
                    {
                        return Problem("No record found");
                    }

                    // Update order properties
                    context.Entry(existingOrder).CurrentValues.SetValues(order);

                    await context.SaveChangesAsync();
                    return Ok(existingOrder);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("insert")]
        public async Task<ActionResult<Order>> PostOrder(Order order, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    context.Order.Add(order);
                    await context.SaveChangesAsync();

                    return Ok(order.OrderId);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId, string token)
        {
            try
            {
                using (var context = new SampleContext(_defaultConnection))
                {
                    var user = await ValidateTokenAndGetUser(token, context);

                    var order = await context.Order.FindAsync(orderId);
                    if (order == null)
                    {
                        return NotFound();
                    }

                    context.Order.Remove(order);
                    await context.SaveChangesAsync();

                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(500, "Internal server error");
            }
        }
    }
}