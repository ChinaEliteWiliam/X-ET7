﻿using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("制作补丁包")]
	public class TaskCreatePatchPackage : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMapContext = context.GetContextObject<BuildMapContext>();
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyPatchFiles(buildParameters, buildMapContext);
			}
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(BuildParametersContext buildParametersContext, BuildMapContext buildMapContext)
		{
			var buildParameters = buildParametersContext.Parameters;
			string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
			string packageOutputDirectory = buildParametersContext.GetPackageOutputDirectory();
			BuildLogger.Log($"开始拷贝补丁文件到补丁包目录：{packageOutputDirectory}");

			if (buildParameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 拷贝构建日志
				{
					string sourcePath = $"{pipelineOutputDirectory}/buildlogtep.json";
					string destPath = $"{packageOutputDirectory}/buildlogtep.json";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝代码防裁剪配置
				if (buildParameters.SBPParameters.WriteLinkXML)
				{
					string sourcePath = $"{pipelineOutputDirectory}/link.xml";
					string destPath = $"{packageOutputDirectory}/link.xml";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}
			else if (buildParameters.BuildPipeline == EBuildPipeline.BuiltinBuildPipeline)
			{
				// 拷贝UnityManifest序列化文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
					string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝UnityManifest文本文件
				{
					string sourcePath = $"{pipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					string destPath = $"{packageOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}
			else
			{
				throw new System.NotImplementedException();
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			int patchFileTotalCount = buildMapContext.BundleInfos.Count;
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				EditorTools.CopyFile(bundleInfo.PatchInfo.BuildOutputFilePath, bundleInfo.PatchInfo.PatchOutputFilePath, true);
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}