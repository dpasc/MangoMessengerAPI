﻿namespace MangoAPI.Presentation.Controllers
{
    using BusinessLogic.ApiCommands.Users;
    using BusinessLogic.ApiQueries.Users;
    using BusinessLogic.Responses;
    using Extensions;
    using Interfaces;
    using MangoAPI.BusinessLogic.ApiQueries.Contacts;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Controller responsible for User Entity.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ApiControllerBase, IUsersController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="mediator">Instance of mediator.</param>
        public UsersController(IMediator mediator)
            : base(mediator)
        {
        }

        /// <summary>
        /// Registers user in the system. There are two possibilities to verify account: Phone (1), Email (2).
        /// Does not require any authorization or users role.
        /// After registration user receives pair of access/refresh tokens.
        /// Access token claim role is Unverified.
        /// </summary>
        /// <param name="request">Request instance.</param>
        /// <param name="cancellationToken">Cancellation token instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(Summary =
            "Registers user in the system. There are two possibilities to verify account: Phone (1), Email (2). " +
            "Does not require any authorization or users role. " +
            "After registration user receives pair of access/refresh tokens. " +
            "Access token claim role is Unverified. ")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request,
            CancellationToken cancellationToken)
        {
            return await RequestAsync(request.ToCommand(), cancellationToken);
        }

        /// <summary>
        /// Confirms user's email address. Adds a User role to the current user.
        /// This endpoint may be accessed by both roles: Unverified, User.
        /// On refresh session user receives new access token with updated roles.
        /// </summary>
        /// <param name="request">VerifyEmailRequest instance.</param>
        /// <param name="cancellationToken">CancellationToken instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpPut("email-confirmation")]
        [Authorize(Roles = "Unverified, User")]
        [SwaggerOperation(Summary = "Confirms user's email address. Adds a User role to the current user. " +
                                    "This endpoint may be accessed by both roles: Unverified, User. " +
                                    "On refresh session user receives new access token with updated roles. ")]
        [ProducesResponseType(typeof(VerifyEmailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> EmailConfirmationAsync([FromBody] VerifyEmailRequest request,
            CancellationToken cancellationToken)
        {
            var command = request.ToCommand();
            return await RequestAsync(command, cancellationToken);
        }

        /// <summary>
        /// Confirms user's phone number. Adds a User role to the current user.
        /// This endpoint may be accessed by both roles: Unverified, User.
        /// On refresh session user receives new access token with updated roles.
        /// </summary>
        /// <param name="phoneCode">Code user enters in order to validate his phone number.</param>
        /// <param name="cancellationToken">CancellationToken instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpPut("{phoneCode:int}")]
        [Authorize(Roles = "Unverified, User")]
        [SwaggerOperation(Summary = "Confirms user's phone number. Adds a User role to the current user. " +
                                    "This endpoint may be accessed by both roles: Unverified, User. " +
                                    "On refresh session user receives new access token with updated roles. ")]
        [ProducesResponseType(typeof(VerifyPhoneResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PhoneConfirmationAsync([FromRoute] int phoneCode,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.GetUserId();
            var command = new VerifyPhoneCommand
            {
                UserId = userId,
                ConfirmationCode = phoneCode,
            };

            return await RequestAsync(command, cancellationToken);
        }

        /// <summary>
        /// Changes password by current password. Required role: User.
        /// </summary>
        /// <param name="request">Request instance.</param>
        /// <param name="cancellationToken">Cancellation Token Instance.</param>
        /// <returns></returns>
        [HttpPut("password")]
        [Authorize(Roles = "User")]
        [SwaggerOperation(Summary = "Changes password by current password. Required role: User")]
        [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.GetUserId();
            var command = request.ToCommand(userId);

            return await RequestAsync(command, cancellationToken);
        }

        /// <summary>
        /// Gets user by ID. Requires role: User.
        /// </summary>
        /// <param name="userId">ID of the user to get, UUID.</param>
        /// <param name="cancellationToken">CancellationToken instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpGet("{userId}")]
        [Authorize(Roles = "User")]
        [SwaggerOperation(Summary = "Gets user by ID. Requires role: User.")]
        [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserById([FromRoute] string userId, CancellationToken cancellationToken)
        {
            var query = new GetUserQuery { UserId = userId };
            return await RequestAsync(query, cancellationToken);
        }

        /// <summary>
        /// Gets info about current user himself.
        /// This endpoint may be accessed by both roles: Unverified, User.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpGet]
        [Authorize(Roles = "Unverified, User")]
        [SwaggerOperation(Summary = "Gets info about current user himself. " +
                                    "This endpoint may be accessed by both roles: Unverified, User.")]
        [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.GetUserId();
            var request = new GetUserQuery { UserId = userId };
            return await RequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Updates user's personal information. Requires role: User.
        /// </summary>
        /// <param name="request">UpdateUserInformationRequest instance.</param>
        /// <param name="cancellationToken">CancellationToken instance.</param>
        /// <returns>Possible codes: 200, 400, 409.</returns>
        [HttpPut("information")]
        [Authorize(Roles = "User")]
        [SwaggerOperation(Summary = "Updates user's personal information. Requires role: User.")]
        [ProducesResponseType(typeof(UpdateUserInformationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserInformationAsync([FromBody] UpdateUserInformationRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.GetUserId();
            var command = request.ToCommand(userId);

            return await RequestAsync(command, cancellationToken);
        }
    }
}