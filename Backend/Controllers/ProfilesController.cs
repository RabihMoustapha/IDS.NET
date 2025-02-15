﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IDS.NET.Repository;
using IDS.NET.Repository.Models;
using System.Security.Cryptography;
using IDS.NET.DTO.Profile;

namespace IDS.NET.Classes
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly idsDbContext _context;

        public ProfilesController(idsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await _context.Profiles.ToListAsync();
        }

        [HttpGet("GetProfileById/{id}")]
        public async Task<ActionResult<Profile>> GetProfile(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        [HttpPut("UpdateName/{id}")]
        public async Task<IActionResult> PutProfile(int id, [FromBody] UpdateNameDTO profileDTO)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            if (id != profile.Id)
            {
                return BadRequest();
            }

            profile.Name = profileDTO.Name;

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost("Create")]
        public async Task<ActionResult<Profile>> PostProfile([FromBody] CreateDTO profileDTO)
        {
            var profile = new Profile
            {
                Name = profileDTO.Name,
                Email = profileDTO.Email,
                Password = profileDTO.Password,
                Token = GenerateToken()
            };
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetProfile", new { id = profile.Id }, profile);
        }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginRequest)
        {
            var profile = await _context.Profiles
                .Where(user => user.Email == loginRequest.Email && user.Password == loginRequest.Password)
                .SingleOrDefaultAsync();

            if (profile == null)
            {
                return NotFound(new { message = "Invalid email or password." });
            }

            _context.Entry(profile).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(profile.Token);
        }

        private string GenerateToken(int length = 32)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var tokenData = new byte[length];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfileExists(int id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }
    }
}
